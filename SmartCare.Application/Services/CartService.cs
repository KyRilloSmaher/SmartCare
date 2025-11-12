
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
            _inventory_repository_guard(inventoryRepository);
            _inventoryRepository = inventoryRepository;
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

        // small guard to avoid parameter name conflict in assignment above
        private void _inventory_repository_guard(IInventoryRepository repository) { if (repository == null) throw new ArgumentNullException(nameof(repository)); }

        #endregion

        #region Public Methods

        public async Task<Response<CartResponseDto?>> GetCartByIdAsync(Guid cartId)
        {
            

            if (cartId == Guid.Empty)
                return _responseHandler.BadRequest<CartResponseDto?>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null)
            {
               
                return _responseHandler.NotFound<CartResponseDto?>(SystemMessages.NOT_FOUND);
            }

            var dto = _mapper.Map<CartResponseDto?>(cart);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<CartResponseDto>> GetUserActiveCartAsync(string userId)
        {
           

            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetActiveCartAsync(userId);
            if (cart == null)
            {
                
                return _responseHandler.NotFound<CartResponseDto>(SystemMessages.NOT_FOUND);
            }

            var dto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<Guid>> CreateCartForUserAsync(string userId)
        {
           
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<Guid>(SystemMessages.BAD_REQUEST);

            var existing = await _cartRepository.GetActiveCartAsync(userId);
            if (existing != null)
            {
               
                return _responseHandler.Success(existing.Id, SystemMessages.CART_ALREADY_EXISTS);
            }

            var newCart = await _cartRepository.CreateCartAsync(userId);
            return _responseHandler.Success(newCart.Id, SystemMessages.CART_CREATED);
        }

        public async Task<Response<CartItemResponseDto?>> AddToCartAsync(AddToCartRequestDto dto)
        {
            _logger.LogInformation("AddToCartAsync request CartId={CartId} ProductId={ProductId} Qty={Quantity}",
                dto.CartId, dto.ProductId, dto.Quantity);

            // Validate cart and product
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found for id {CartId}", dto.CartId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);
            }

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for id {ProductId}", dto.ProductId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            // Acquire lock with retry on TimeoutException
            return await _lockRetryPolicy.ExecuteAsync(async () =>
            {
                await using var appLock = await _sqlLockManager.AcquireLockAsync(
                    $"Inventory-{product.ProductId}",
                    mode: "Exclusive",
                    timeoutMs: 10000);

                // Begin transaction
                await _cartRepository.BeginTransactionAsync();

                var eventsToPublish = new List<object>();
                try
                {
                    // Re-check stock inside lock
                    var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
                    if (availableStock < dto.Quantity)
                    {
                        _logger.LogInformation("Insufficient stock for ProductId={ProductId}. Needed={Needed}, Available={Available}",
                            product.ProductId, dto.Quantity, availableStock);
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                    }

                    var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.Quantity);
                    if (inventoryId == Guid.Empty)
                    {
                        _logger.LogWarning("No inventory chunk found for product {ProductId} qty {Qty}", product.ProductId, dto.Quantity);
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                    }

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

                    var reservation = await _reservationRepository.CreateReservationAsync(cartItem, dto.Quantity, ReservationStatus.ReservedUntilCheckout);
                    if (reservation == null)
                    {
                        _logger.LogWarning("Reservation creation failed for cartItem {CartItemId}", cartItem.CartItemId);
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                    }

                    cartItem.ReservationId = reservation.Id;
                    cart.TotalPrice += cartItem.SubTotal;

                    var added = await _cartRepository.AddCartItemAsync(cartItem);
                    if (!added)
                    {
                        _logger.LogError("Failed to add CartItem to repository for CartId={CartId} ProductId={ProductId}", cart.Id, product.ProductId);
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                    }

                    // Commit transaction before side-effects
                    await _cartRepository.CommitTransactionAsync();

                    // Prepare event(s) to publish post-commit
                    eventsToPublish.Add(new ProductStockStatusChangedEvent(product.ProductId, availableStock - dto.Quantity > 0));

                    // Publish events asynchronously using background job (post-commit)
                    _backgroundJobService.Enqueue(() => PublishEventsAsync(eventsToPublish));

                    // Schedule reservation cleanup (post-commit)
                    _backgroundJobService.Schedule(
                        () => _reservationRepository.CancelReservationAsync(reservation, ReservationStatus.Realesed),
                        TimeSpan.FromMinutes(10));

                    var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
                    _logger.LogInformation("Added item to cart {CartId} CartItemId={CartItemId} ProductId={ProductId}",
                        cart.Id, cartItem.CartItemId, product.ProductId);
                    return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
                }
                catch (TimeoutException tex)
                {
                    // Let retry policy handle this (rethrow)
                    _logger.LogWarning(tex, "Timeout while adding to cart for ProductId={ProductId}", product.ProductId);
                    await _cartRepository.RollBackAsync();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while adding to cart for ProductId={ProductId}", product.ProductId);
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }
            });
        }

        public async Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto)
        {
            _logger.LogInformation("UpdateCartItemQuantityAsync CartId={CartId}, CartItemId={CartItemId}, ProductId={ProductId}, NewQty={NewQty}",
                dto.CartId, dto.CartItemId, dto.ProductId, dto.NewQuantity);

            // Step 1: validate
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found: {CartId}", dto.CartId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);
            }

            var reservation = await EnsureReservationExistsAsync(dto.ReservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found: {ReservationId}", dto.ReservationId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);
            }

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
            {
                _logger.LogWarning("Cart item not found cartItemId={CartItemId} productId={ProductId}", dto.CartItemId, dto.ProductId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);
            }

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", dto.ProductId);
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            // Acquire lock + transaction similar to AddToCart
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
                    if (dto.NewQuantity > cartItem.Quantity)
                    {
                        var extraNeeded = dto.NewQuantity - cartItem.Quantity;
                        var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);

                        if (availableStock < extraNeeded)
                        {
                            _logger.LogInformation("Insufficient stock to increase quantity for ProductId={ProductId}. ExtraNeeded={ExtraNeeded}, Available={Available}",
                                product.ProductId, extraNeeded, availableStock);
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                        }

                        var inventoryId = await _inventory_repository_getBest(product.ProductId, extraNeeded);
                        if (inventoryId == Guid.Empty)
                        {
                            _logger.LogWarning("No inventory chunk available to cover extra quantity for product {ProductId}", product.ProductId);
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                        }

                        var updatedReservation = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                        if (!updatedReservation)
                        {
                            _logger.LogError("Failed to update reservation quantity for reservation {ReservationId}", reservation.Id);
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                        }

                        eventsToPublish.Add(new ProductStockStatusChangedEvent(product.ProductId, availableStock - dto.NewQuantity > 0));
                    }
                    else if (dto.NewQuantity < cartItem.Quantity)
                    {
                        // release difference by updating reservation
                        var updated = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                        if (!updated)
                        {
                            _logger.LogError("Failed to decrease reservation quantity for reservation {ReservationId}", reservation.Id);
                            return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                        }

                        // optionally: do nothing else here — Publish stock status later
                        eventsToPublish.Add(new ProductStockStatusChangedEvent(product.ProductId,
                            await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId) > 0));
                    }

                    // Update cart item domain using mapper
                    _mapper.Map(dto, cartItem);
                    cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;
                    cart.TotalPrice = await _cartRepository.CalculateCartTotalAsync(cart.Id);

                    var updatedItem = await _cartRepository.UpdateItemCartAsync(cartItem);
                    if (!updatedItem)
                        return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);

                    await _cartRepository.CommitTransactionAsync();

                    // Post-commit — publish events
                    if (eventsToPublish.Count > 0)
                    {
                        _backgroundJobService.Enqueue(() => PublishEventsAsync(eventsToPublish));
                    }

                    var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
                    _logger.LogInformation("Updated cart item {CartItemId} quantity to {NewQty}", cartItem.CartItemId, dto.NewQuantity);
                    return _responseHandler.Success(responseDto, SystemMessages.CART_UPDATED);
                }
                catch (TimeoutException tEx)
                {
                    _logger.LogWarning(tEx, "Timeout updating cart item quantity for ProductId {ProductId}", product.ProductId);
                    await _cartRepository.RollBackAsync();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error updating cart item quantity");
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }
            });
        }

        public async Task<Response<bool>> RemoveFromCartAsync(RemoveFromCartRequestDto dto)
        {

            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found when removing item: {CartId}", dto.CartId);
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            }
            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
            {
                _logger.LogWarning("CartItem not found for removal: {CartItemId}", dto.CartItemId);
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            }
            var reservation = await EnsureReservationExistsAsync(cartItem.ReservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found when removing item: {ReservationId}", cartItem.ReservationId);
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            }



            await _cartRepository.BeginTransactionAsync();
            try
            {
                var removed = await _cartRepository.RemoveCartItemAsync(cartItem);
                if (!removed)
                    return await FailTransactionAsync<bool>(SystemMessages.SERVER_ERROR);

                var cancelled = await _reservationRepository.CancelReservationAsync(reservation, ReservationStatus.Realesed);
                if (!cancelled)
                    return await FailTransactionAsync<bool>(SystemMessages.FAILED);

                await _cartRepository.CommitTransactionAsync();



                // Post-commit publish: product stock status
                var available = await _inventoryRepository.GetTotalStockForProductAsync(cartItem.ProductId);
                var eventModel = new ProductStockStatusChangedEvent(cartItem.ProductId, available > 0);
                _backgroundJobService.Enqueue(() =>_eventBus.PublishAsync(eventModel));

               

                _logger.LogInformation("Removed item {CartItemId} from cart {CartId}", dto.CartItemId, dto.CartId);
                return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart {CartId}", dto.CartId);
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<bool>(SystemMessages.SERVER_ERROR);
            }
        }

        public async Task<Response<bool>> ClearCartAsync(Guid cartId)
        {
            _logger.LogInformation("ClearCartAsync called CartId={CartId}", cartId);

            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found when clearing: {CartId}", cartId);
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            }

            // Read items first to prepare events
            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            var isCleared = await _cartRepository.ClearCartAsync(cart);

            if (!isCleared)
            {
                _logger.LogError("Failed to clear cart {CartId}", cartId);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }

            // Batch product stock status events
            var stockEvents = new List<ProductStockStatusChangedEvent>();
            foreach (var item in cartItems)
            {
                var available = await _inventoryRepository.GetTotalStockForProductAsync(item.ProductId);
                stockEvents.Add(new ProductStockStatusChangedEvent(item.ProductId, available > 0));
                var reservation = await EnsureReservationExistsAsync(item.ReservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation not found when removing item: {ReservationId}", item.ReservationId);
                    return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
                }
                _backgroundJobService.Enqueue(() => _reservationRepository.CancelReservationAsync(reservation, ReservationStatus.Realesed));
            }
            _backgroundJobService.Enqueue(() =>PublishEventsAsync(stockEvents.ConvertAll(e => (object)e)));
            _logger.LogInformation("Cart {CartId} cleared and cleanup jobs enqueued", cartId);
            return _responseHandler.Success(true, SystemMessages.CART_CLEARED);
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

            // also enqueue background cleanups
            _backgroundJobService.Enqueue(() => _reservationRepository.ReleaseAllReservationsForCartAsync(cartId));
            _backgroundJobService.Enqueue(() => UpdateCartProductsStatus(cartId));

            _logger.LogInformation("Deleted cart {CartId}", cartId);
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        #endregion

        #region Private Helpers

        private async Task<Cart?> EnsureCartExistsAsync(Guid cartId) =>
            await _cartRepository.GetByIdAsync(cartId);

        private async Task<Product?> EnsureProductExistsAsync(Guid productId) =>
            await _productRepository.GetByIdAsync(productId);

        private async Task<Reservation?> EnsureReservationExistsAsync(Guid reservationId) =>
            await _reservationRepository.GetByIdAsync(reservationId);

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
                    var available = await _inventoryRepository.GetTotalStockForProductAsync(item.ProductId);
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

        // small wrapper to match previous call style when repositories were used inline
        private async Task<Guid> _inventory_repository_getBest(Guid productId, int quantity)
        {
            return await _inventoryRepository.GetBestInventoryIdAsync(productId, quantity);
        }

        #endregion
    }
}
