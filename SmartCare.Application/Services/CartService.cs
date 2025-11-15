using AutoMapper;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SmartCare.Application.commons;
using SmartCare.Application.Commons;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Events;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{
    public class CartService : ICartService
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISqlLockManager _sqlLockManager;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly ILogger<CartService> _logger;

        private readonly AsyncRetryPolicy _lockRetryPolicy;

        #endregion

        #region Constructor

        public CartService(
            IResponseHandler responseHandler,
            ICartRepository cartRepository,
            IReservationRepository reservationRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IBackgroundJobService backgroundJobService,
            ISqlLockManager sqlLockManager,
            IInventoryRepository inventoryRepository,
            IEventBus eventBus,
            ILogger<CartService> logger)
        {
            _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
            _sqlLockManager = sqlLockManager ?? throw new ArgumentNullException(nameof(sqlLockManager));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Retry policy for lock acquisition: handle TimeoutException, retry a few times with exponential backoff
            _lockRetryPolicy = Policy
                .Handle<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(250 * attempt),
                    onRetry: (ex, wait, retryCount, context) =>
                    {
                        _logger.LogWarning(ex, "Lock acquisition retry {Retry} after {Wait}ms", retryCount, wait.TotalMilliseconds);
                    });
        }

        #endregion

        #region Public Methods

        public async Task<Response<CartResponseDto?>> GetCartByIdAsync(Guid cartId)
        {
            if (cartId == Guid.Empty)
                return _responseHandler.BadRequest<CartResponseDto?>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto?>(SystemMessages.NOT_FOUND);

            var dto = _mapper.Map<CartResponseDto?>(cart);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<CartResponseDto>> GetUserActiveCartAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetActiveCartAsync(userId);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto>(SystemMessages.NOT_FOUND);

            var dto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<Guid>> CreateCartForUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<Guid>(SystemMessages.BAD_REQUEST);

            var existing = await _cartRepository.GetActiveCartAsync(userId);
            if (existing != null)
                return _responseHandler.Success(existing.Id, SystemMessages.CART_ALREADY_EXISTS);

            var newCart = await _cartRepository.CreateCartAsync(userId);
            return _responseHandler.Success(newCart.Id, SystemMessages.CART_CREATED);
        }

        public async Task<Response<CartItemResponseDto?>> AddToCartAsync(AddToCartRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            return await _lockRetryPolicy.ExecuteAsync(async () =>
            {
                await using var appLock = await _sqlLockManager.AcquireLockAsync(
                    $"Inventory-{product.ProductId}", "Exclusive", 10000);

                await _cartRepository.BeginTransactionAsync();
                var eventsToPublish = new List<object>();

                try
                {
                    // 1️⃣ Check available stock
                    var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
                    if (availableStock < dto.Quantity)
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

                    var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.Quantity);
                    if (inventoryId == Guid.Empty)
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

                    // 2️⃣ Create and save CartItem first
                    var cartItem = new CartItem
                    {
                        CartItemId = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = product.ProductId,
                        InventoryId = inventoryId,
                        Quantity = dto.Quantity,
                        UnitPrice = product.Price,
                        SubTotal = product.Price * dto.Quantity
                    };

                    await _cartRepository.AddCartItemAsync(cartItem);
                    await _cartRepository.SaveChangesAsync(); // ✅ Save to generate CartItemId for FK

                    // 3️⃣ Create Reservation using the saved CartItem
                    var reservation = await _reservationRepository.CreateReservationAsync(
                        cartItem, dto.Quantity, ReservationStatus.ReservedUntilCheckout);

                    if (reservation == null)
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);

                    cartItem.ReservationId = reservation.Id;
                    await _cartRepository.UpdateItemCartAsync(cartItem);

                    await _cartRepository.CalculateCartTotalAsync(cart.Id);
                    await _cartRepository.CommitTransactionAsync();

                    // 4️⃣ Post-commit jobs
                    eventsToPublish.Add(new ProductStockStatusChangedEvent(product.ProductId, availableStock - dto.Quantity > 0));
                    _backgroundJobService.Enqueue(() => PublishEventsAsync(eventsToPublish));
                    _backgroundJobService.Schedule(
                        () => CancelReservationPrivate(reservation.Id, inventoryId, cart.Id, product.ProductId, cartItem.Quantity),
                        TimeSpan.FromMinutes(10));

                    var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
                    return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
                }
                catch (Exception ex)
                {
                    await _cartRepository.RollBackAsync();
                    _logger.LogError(ex, "Error in AddToCartAsync for Cart {CartId} Product {ProductId}", dto.CartId, dto.ProductId);
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }
            });
        }


        public async Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);

            var reservation = await EnsureReservationExistsAsync(cartItem.ReservationId);
            if (reservation == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);

            var product = await EnsureProductExistsAsync(cartItem.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            return await _lockRetryPolicy.ExecuteAsync(async () =>
            {
                await using var appLock = await _sqlLockManager.AcquireLockAsync(
                    $"Inventory-{product.ProductId}",
                    mode: "Exclusive",
                    timeoutMs: 10000);

                await _cartRepository.BeginTransactionAsync();
                var eventsToPublish = new List<object>();

                try
                {
                    int quantityDiff = dto.NewQuantity - cartItem.Quantity;

                    if (quantityDiff > 0)
                    {
                        // Increasing quantity: check stock
                        var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
                        if (availableStock < quantityDiff)
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

                        var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, quantityDiff);
                        if (inventoryId == Guid.Empty)
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                    }

                    // Update reservation
                    var updated = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                    if (!updated)
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);

                    // Update cart item
                    cartItem.Quantity = dto.NewQuantity;
                    cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;
                    await _cartRepository.UpdateItemCartAsync(cartItem);
                    await _cartRepository.CalculateCartTotalAsync(cart.Id);

                    await _cartRepository.CommitTransactionAsync();

                    eventsToPublish.Add(new ProductStockStatusChangedEvent(product.ProductId,
                        await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId) > 0));

                    _backgroundJobService.Enqueue(() => PublishEventsAsync(eventsToPublish));

                    var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
                    return _responseHandler.Success(responseDto, SystemMessages.CART_UPDATED);
                }
                catch (Exception ex)
                {
                    await _cartRepository.RollBackAsync();
                    _logger.LogError(ex, "Error updating cart item {CartItemId} in Cart {CartId}", dto.CartItemId, dto.CartId);
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }
            });
        }

        public async Task<Response<bool>> RemoveFromCartAsync(RemoveFromCartRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var reservation = await EnsureReservationExistsAsync(cartItem.ReservationId);
            if (reservation == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            return await _lockRetryPolicy.ExecuteAsync(async () =>
            {
                await using var appLock = await _sqlLockManager.AcquireLockAsync(
                    $"Inventory-{cartItem.ProductId}",
                    mode: "Exclusive",
                    timeoutMs: 10000);

                await _cartRepository.BeginTransactionAsync();

                try
                {
                    // Remove cart item and update reservation/stock
                    var removed = await _cartRepository.RemoveCartItemAsync(cartItem);
                    if (!removed)
                        return await FailTransactionAsync<bool>(SystemMessages.SERVER_ERROR);

                    await _cartRepository.CalculateCartTotalAsync(cart.Id);

                    await _reservationRepository.CancelReservationAsync(reservation.Id, cartItem.InventoryId, ReservationStatus.Realesed);

                    await _cartRepository.CommitTransactionAsync();

                    // Post-commit events
                    var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(cartItem.ProductId);
                    _backgroundJobService.Enqueue(() =>
                        _eventBus.PublishAsync(new ProductStockStatusChangedEvent(cartItem.ProductId, availableStock > 0)));

                    return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);
                }
                catch (Exception ex)
                {
                    await _cartRepository.RollBackAsync();
                    _logger.LogError(ex, "Error removing item {CartItemId} from cart {CartId}", dto.CartItemId, dto.CartId);
                    return _responseHandler.Failed<bool>(SystemMessages.SERVER_ERROR);
                }
            });
        }

        public async Task<Response<bool>> ClearCartAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.Success(true, SystemMessages.CART_CLEARED);

            return await _lockRetryPolicy.ExecuteAsync(async () =>
            {
                await using var appLock = await _sqlLockManager.AcquireLockAsync(
                    $"Cart-{cartId}",
                    mode: "Exclusive",
                    timeoutMs: 10000);

                await _cartRepository.BeginTransactionAsync();

                try
                {
                    foreach (var item in cartItems)
                    {
                        await _cartRepository.RemoveCartItemAsync(item);
                        await _reservationRepository.CancelReservationAsync(item.ReservationId, item.InventoryId, ReservationStatus.Realesed);
                    }

                    await _cartRepository.CalculateCartTotalAsync(cart.Id);

                    await _cartRepository.CommitTransactionAsync();

                    // Post-commit events
                    var stockEvents = new List<ProductStockStatusChangedEvent>();
                    foreach (var item in cartItems)
                    {
                        var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(item.ProductId);
                        stockEvents.Add(new ProductStockStatusChangedEvent(item.ProductId, availableStock > 0));
                    }

                    _backgroundJobService.Enqueue(() => PublishEventsAsync(stockEvents.ConvertAll(e => (object)e)));

                    return _responseHandler.Success(true, SystemMessages.CART_CLEARED);
                }
                catch (Exception ex)
                {
                    await _cartRepository.RollBackAsync();
                    _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
                    return _responseHandler.Failed<bool>(SystemMessages.SERVER_ERROR);
                }
            });
        }


        public async Task<Response<IEnumerable<CartItemResponseDto>>> GetCartItemsAsync(Guid cartId)
        {
            _logger.LogDebug("GetCartItemsAsync called for CartId={CartId}", cartId);

            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found for GetCartItemsAsync: {CartId}", cartId);
                return _responseHandler.NotFound<IEnumerable<CartItemResponseDto>>(SystemMessages.NOT_FOUND);
            }

            var items = await _cartRepository.GetCartItemsAsync(cart.Id);
            var dto = _mapper.Map<IEnumerable<CartItemResponseDto>>(items);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<bool>> DeleteCartAsync(Guid cartId)
        {
            _logger.LogInformation("DeleteCartAsync called for CartId={CartId}", cartId);

            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found for delete: {CartId}", cartId);
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            }

            var deleted = await _cartRepository.DeleteAsync(cart);
            if (!deleted)
            {
                _logger.LogError("Failed to delete cart {CartId}", cartId);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }

            // enqueue background cleanups
            _backgroundJobService.Enqueue(() => _reservationRepository.ReleaseAllReservationsForCartAsync(cartId));
            _backgroundJobService.Enqueue(() => UpdateCartProductsStatus(cartId));

            _logger.LogInformation("Deleted cart {CartId}", cartId);
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        #endregion

        #region Private Helpers

        private async Task<Cart?> EnsureCartExistsAsync(Guid cartId, bool track = false) =>
            await _cartRepository.GetByIdAsync(cartId, track);

        private async Task<Product?> EnsureProductExistsAsync(Guid productId) =>
            await _productRepository.GetByIdAsync(productId);

        private async Task<Reservation?> EnsureReservationExistsAsync(Guid reservationId, bool track = false) =>
            await _reservationRepository.GetByIdAsync(reservationId, track);

        private async Task<Response<T>> FailTransactionAsync<T>(string message)
        {
            try
            {
                await _cartRepository.RollBackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rollback failed while failing transaction with message: {Message}", message);
            }

            return _responseHandler.Failed<T>(message);
        }

        private async Task UpdateCartProductsStatus(Guid cartId)
        {
            try
            {
                var cartItems = await _cartRepository.GetCartItemsAsync(cartId);
                var events = new List<object>();
                foreach (var item in cartItems)
                {
                    var available = await _inventory_repository_getTotalStock(item.ProductId);
                    events.Add(new ProductStockStatusChangedEvent(item.ProductId, available > 0));
                }

                await PublishEventsAsync(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart products status for cart {CartId}", cartId);
            }
        }

        public async Task PublishEventsAsync(IEnumerable<object> events)
        {
            foreach (var evt in events)
            {
                try
                {
                    await _eventBus.PublishAsync(evt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed publishing event {EventType}", evt?.GetType().Name ?? "Unknown");
                    // don't rethrow: publishing failure shouldn't block user flow (we have retry or DLQ at infra level)
                }
            }
        }

        // small wrapper kept for clarity/consistency
        private async Task<Guid> _inventory_repository_getBest(Guid productId, int quantity)
        {
            return await _inventoryRepository.GetBestInventoryIdAsync(productId, quantity);
        }

        private async Task<int> _inventory_repository_getTotalStock(Guid productId)
        {
            return await _inventoryRepository.GetTotalStockForProductAsync(productId);
        }

        /// <summary>
        /// Cancel reservation invoked by background job when reservation TTL expires.
        /// </summary>
        public async Task CancelReservationPrivate(Guid reservationId, Guid inventoryId, Guid CartId, Guid productId, int quantity)
        {
            try
            {
                bool done = await _reservationRepository.CancelReservationAsync(reservationId, inventoryId, ReservationStatus.Realesed);
                if (!done)
                {
                    _logger.LogError("Failed to cancel reservation {ReservationId}", reservationId);
                }

                // recalc cart total if cartId known - try to find cart containing reservation (best-effort)
                try
                {
                    var cart = await _cartRepository.GetByIdAsync(CartId);
                    if (cart != null)
                    {
                        await _cartRepository.CalculateCartTotalAsync(cart.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to calculate cart total after cancelling reservation {ReservationId}", reservationId);
                }

                // publish ReservationExpiredEvent (post-commit via background job)
                var eventsToPublish = new List<object>
                {
                    new ReservationExpiredEvent(CartId, productId, quantity, "Your reservation has expired and the item was removed from your cart.")
                };

                _backgroundJobService.Enqueue(() => PublishEventsAsync(eventsToPublish));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CancelReservationPrivate for reservation {ReservationId}", reservationId);
            }
        }

        #endregion
    }
}
