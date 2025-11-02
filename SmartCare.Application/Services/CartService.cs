using AutoMapper;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
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
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _backgroundJobService;

        #endregion

        #region Constructor

        public CartService(
            IResponseHandler responseHandler,
            ICartRepository cartRepository,
            IReservationRepository reservationRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IBackgroundJobService backgroundJobService)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _reservationRepository = reservationRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _backgroundJobService = backgroundJobService;
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
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);

            var product = await EnsureProductExistsAsync(dto.ProductId);
            if (product == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            // Step 1: Check stock and get available inventory
            //var availableStock = await _productRepository.GetAvailableStockAsync(product.ProductId);
            //if (availableStock < dto.Quantity)
            //    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

            //var inventoryId = await _productRepository.GetAvailableInventoryIdAsync(product.ProductId, dto.Quantity);
            //if (inventoryId == Guid.Empty)
            //    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

            var cartItem = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.ProductId,
               // InventoryId = inventoryId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price,
                SubTotal = product.Price * dto.Quantity
            };

            await _cartRepository.BeginTransactionAsync();
            try
            {
                // Step 2: Create reservation
                var reservation = await _reservationRepository.CreateReservationAsync(cartItem.CartItemId, dto.Quantity);
                if (reservation == null)
                    return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.RESERVATION_FAILED);

                cartItem.ReservationId = reservation.Id;

                // Step 3: Add item & update total
                cart.TotalPrice += cartItem.SubTotal;
                var isAdded = await _cartRepository.AddCartItemAsync(cartItem);
                if (!isAdded)
                    return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);

                // Step 4: Commit transaction
                await _cartRepository.CommitTransactionAsync();

                // Step 5: Schedule reservation expiration cleanup (runs outside transaction)
                _backgroundJobService.Schedule(
                    () => _reservationRepository.CancelReservationAsync(reservation),
                    TimeSpan.FromMinutes(10));

                var dtoResponse = _mapper.Map<CartItemResponseDto>(cartItem);
                return _responseHandler.Success(dtoResponse, SystemMessages.ADDED_TO_CART);
            }
            catch
            {
                await _cartRepository.RollBackAsync();
                return _responseHandler.Failed<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);
            }
        }

        public async Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);

            var reservation = await EnsureReservationExistsAsync(dto.ReservationId);
            if (reservation == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);
            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId, dto.ProductId);
            if (cartItem == null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.NOT_FOUND);
            _mapper.Map(dto, cartItem);
            await _cartRepository.BeginTransactionAsync();
            try
            {
                var isUpdated = await _cartRepository.UpdateItemCartAsync(cartItem);
                if (!isUpdated)
                    return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.SERVER_ERROR);

                var isReservationUpdated = await _reservationRepository.UpdateReservationQuantityAsync(reservation, dto.NewQuantity);
                if (!isReservationUpdated)
                    return await FailTransactionAsync<CartItemResponseDto?>(SystemMessages.FAILED);

                await _cartRepository.CommitTransactionAsync();
                var dtoResponse = _mapper.Map<CartItemResponseDto?>(cartItem);
                return _responseHandler.Success(dtoResponse, SystemMessages.CART_UPDATED);
            }
            catch
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

        #endregion
    }
}
