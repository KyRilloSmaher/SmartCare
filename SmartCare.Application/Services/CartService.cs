using AutoMapper;
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
            IEventBus eventBus)
        {

            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _reservationRepository = reservationRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _backgroundJobService = backgroundJobService;
            _sqlLockManager = sqlLockManager;
            _inventoryRepository = inventoryRepository;
            _eventBus = eventBus;
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

            var cartDto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(cartDto);
        }

        public async Task<Response<CartResponseDto>> GetUserActiveCartAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetActiveCartAsync(userId);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto>(SystemMessages.NOT_FOUND);

            var cartDto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(cartDto);
        }

        public async Task<Response<Guid>> CreateCartForUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<Guid>(SystemMessages.BAD_REQUEST);

            var existingCart = await _cartRepository.GetActiveCartAsync(userId);
            if (existingCart != null)
                return _responseHandler.Success(existingCart.Id, SystemMessages.CART_ALREADY_EXISTS);

            var newCart = await _cartRepository.CreateCartAsync(userId);
            return _responseHandler.Success(newCart.Id, SystemMessages.CART_CREATED);
        }

        public async Task<Response<CartItemResponseDto?>> AddToCartAsync(AddToCartRequestDto dto)
        {
            // Step 1: Validate cart and product existence
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            // Step 2: Acquire SQL application lock 
            await using var appLock = await _sqlLockManager.AcquireLockAsync(
                $"Inventory-{product.ProductId}",
                mode: "Exclusive",
                timeoutMs: 10000);

            await _cartRepository.BeginTransactionAsync();
            try
            {
                // Step 3: Check stock again (inside lock)
                var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
                if (availableStock < dto.Quantity)
                {
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                }

                // Step 4: Pick inventory (safe within lock)
                var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.Quantity);
                if (inventoryId == Guid.Empty)
                {
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                }

                // Step 5: Create Cart Item
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

                // Step 6: Create Reservation
                var reservation = await _reservationRepository.CreateReservationAsync(cartItem.CartItemId, dto.Quantity);
                if (reservation == null)
                {
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                }
                await _eventBus.PublishAsync(new ProductStockStatusChangedEvent(
                                     product.ProductId, availableStock - dto.Quantity > 0
                                ));
                cartItem.ReservationId = reservation.Id;
                cart.TotalPrice += cartItem.SubTotal;

                // Step 7: Save Cart Item
                var isAdded = await _cartRepository.AddCartItemAsync(cartItem);
                if (!isAdded)
                {
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                // Step 8: Commit transaction
                await _cartRepository.CommitTransactionAsync();

                // Step 9: Schedule reservation cleanup (outside transaction)
                _backgroundJobService.Schedule(
                    () => _reservationRepository.CancelReservationAsync(reservation),
                    TimeSpan.FromMinutes(10));

                var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
                return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
            }
            catch (TimeoutException)
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.OPERATION_TIMEOUT);
            }
            catch (Exception)
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
            }
        }

        public async Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto)
        {
            // Step 1: Validate entities
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var reservation = await EnsureReservationExistsAsync(dto.ReservationId);
            if (reservation == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId, dto.ProductId);
            if (cartItem == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            // Step 2: Acquire SQL application lock for this product
            await using var appLock = await _sqlLockManager.AcquireLockAsync(
                $"Inventory-{product.ProductId}",
                mode: "Exclusive",
                timeoutMs: 10000);

            await _cartRepository.BeginTransactionAsync();
            try
            {
                // Step 3: Handle stock adjustments inside the lock
                if (dto.NewQuantity > cartItem.Quantity)
                {
                    // Need to increase reservation → check stock availability
                    var extraNeeded = dto.NewQuantity - cartItem.Quantity;
                    var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);

                    if (availableStock < extraNeeded)
                    {
                        await _cartRepository.RollBackAsync();
                        return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                    }

                    // Optionally pick inventory again if necessary
                    var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, extraNeeded);
                    if (inventoryId == Guid.Empty)
                    {
                        await _cartRepository.RollBackAsync();
                        return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                    }

                    // Update reservation (add extra quantity)
                    var updatedReservation = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                    if (!updatedReservation)
                    {
                        await _cartRepository.RollBackAsync();
                        return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                    }
                    await _eventBus.PublishAsync(new ProductStockStatusChangedEvent(
                                   product.ProductId, availableStock - dto.NewQuantity > 0
                              ));
                }
                else if (dto.NewQuantity < cartItem.Quantity)
                {
                    // Decrease reservation (release stock)
                    var updatedReservation = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                    if (!updatedReservation)
                    {
                        await _cartRepository.RollBackAsync();
                        return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);
                    }
                }

                // Step 4: Update cart item details
                _mapper.Map(dto, cartItem);
                cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;
                cart.TotalPrice = await _cartRepository.CalculateCartTotalAsync(cart.Id);

                var isUpdated = await _cartRepository.UpdateItemCartAsync(cartItem);
                if (!isUpdated)
                {
                    await _cartRepository.RollBackAsync();
                    return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
                }

                // Step 5: Commit transaction
                await _cartRepository.CommitTransactionAsync();

                var dtoResponse = _mapper.Map<CartItemResponseDto?>(cartItem);
                return _responseHandler.Success(dtoResponse, SystemMessages.CART_UPDATED);
            }
            catch (TimeoutException)
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.OPERATION_TIMEOUT);
            }
            catch (Exception)
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
            }
        }

        public async Task<Response<bool>> RemoveFromCartAsync(RemoveFromCartRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var reservation = await EnsureReservationExistsAsync(dto.ReservationId);
            if (reservation == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId, dto.ProductId);
            if (cartItem == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            await _cartRepository.BeginTransactionAsync();
            try
            {
                var isRemoved = await _cartRepository.RemoveCartItemAsync(cartItem);
                if (!isRemoved)
                    return await FailTransactionAsync<bool>(SystemMessages.SERVER_ERROR);

                var isReservationCancelled = await _reservationRepository.CancelReservationAsync(reservation);
                if (!isReservationCancelled)
                    return await FailTransactionAsync<bool>(SystemMessages.FAILED);

                await _cartRepository.CommitTransactionAsync();
                await _eventBus.PublishAsync(new ProductStockStatusChangedEvent(
                       cartItem.ProductId, await _inventoryRepository.GetTotalStockForProductAsync(cartItem.ProductId) > 0
                  ));
                return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);
            }
            catch
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<bool>(SystemMessages.SERVER_ERROR);
            }
        }

        public async Task<Response<bool>> ClearCartAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
             
              
             var isCleared = await _cartRepository.ClearCartAsync(cart);
            _backgroundJobService.Enqueue(() => _reservationRepository.ReleaseAllReservationsForCartAsync(cartId));
            _backgroundJobService.Enqueue(() =>  UpdateCartProductsStatus(cartId));
            return isCleared
                ? _responseHandler.Success(true, SystemMessages.CART_CLEARED)
                : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<IEnumerable<CartItemResponseDto>>> GetCartItemsAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<IEnumerable<CartItemResponseDto>>(SystemMessages.NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            var cartItemsDto = _mapper.Map<IEnumerable<CartItemResponseDto>>(cartItems);
            return _responseHandler.Success(cartItemsDto);
        }

        public async Task<Response<bool>> DeleteCartAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var isDeleted = await _cartRepository.DeleteAsync(cart);
            return isDeleted
                ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED)
                : _responseHandler.Failed<bool>(SystemMessages.FAILED);
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
            await _cartRepository.RollBackAsync();
            return _responseHandler.Failed<T>(message);
        }
        private async Task UpdateCartProductsStatus(Guid cartId)
        {
            var cartItems = await _cartRepository.GetCartItemsAsync(cartId);
            foreach (var cartItem in cartItems)
            {
                await _eventBus.PublishAsync(new ProductStockStatusChangedEvent(
                     cartItem.ProductId, await _inventoryRepository.GetTotalStockForProductAsync(cartItem.ProductId) > 0
                ));
            }
        }
        #endregion
    }
}
