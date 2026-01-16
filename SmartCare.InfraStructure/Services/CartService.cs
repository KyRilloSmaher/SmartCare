using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SmartCare.Application.Messaging;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Domain.Events;
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
using SmartCare.Application.commens;

namespace SmartCare.InfraStructure.Services
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
        private readonly IConfiguration _configuration;
        private readonly IEventPublisherService _eventPublisherService;
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
            ILogger<CartService> logger,
            IConfiguration configuration,
            IEventPublisherService eventPublisherService)
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
            _configuration = configuration;
            _eventPublisherService = eventPublisherService;
        }

        #endregion

        #region Public Methods

        public async Task<Response<CartResponseDto?>> GetCartByIdAsync(Guid cartId)
        {
            if (cartId == Guid.Empty)
                return _responseHandler.BadRequest<CartResponseDto?>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null || cart.status == CartStatus.Abandoned)
                return _responseHandler.NotFound<CartResponseDto?>(SystemMessages.NOT_FOUND);

            var dto = _mapper.Map<CartResponseDto?>(cart);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<CartResponseDto>> GetUserActiveCartAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetActiveCartAsync(userId);
            if (cart == null || cart.status == CartStatus.Abandoned)
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

            if (await _cartRepository.CheckIfProductExistInCart(dto.CartId, dto.ProductId))
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.PRODUCT_ALREADY_IN_CART);
            var availableStock = await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);
            if (availableStock < dto.Quantity)
                return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            // For First we choose default inventory
            var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.Quantity);
            if (inventoryId == Guid.Empty)
                return  _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);

            // map And Set values coming from domain entities
            var cartItem = _mapper.Map<CartItem>(dto);
            cartItem.CartId = cart.Id;
            cartItem.ProductId = product.ProductId;
            cartItem.UnitPrice = product.Price;
            cartItem.SubTotal = product.Price * cartItem.Quantity;
            cartItem.InventoryId = inventoryId;

            await _cartRepository.AddCartItemAsync(cartItem);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);
            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            return _responseHandler.Success(responseDto, SystemMessages.ADDED_TO_CART);
        }

        public async Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.CART_ITEM_NOT_EXIST);

            var product = await EnsureProductExistsAsync(cartItem.ProductId);
            if (product is null)
                return _responseHandler.NotFound<CartItemResponseDto?>(SystemMessages.PRODUCT_NOT_FOUND);

            var quantityDifference = dto.NewQuantity - cartItem.Quantity;

            if (quantityDifference > 0)
            {
                var availableStock =
                    await _inventoryRepository.GetTotalStockForProductAsync(product.ProductId);

                if (availableStock < quantityDifference)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
                var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(product.ProductId, dto.NewQuantity);
                if (inventoryId == Guid.Empty)
                    return _responseHandler.BadRequest<CartItemResponseDto?>(SystemMessages.INSUFFICIENT_STOCK);
            }

            cartItem.Quantity = dto.NewQuantity;
            cartItem.SubTotal = cartItem.UnitPrice * dto.NewQuantity;

            await _cartRepository.UpdateItemCartAsync(cartItem);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);

            var responseDto = _mapper.Map<CartItemResponseDto?>(cartItem);
            return _responseHandler.Success(responseDto, SystemMessages.CART_UPDATED);
        }

        public async Task<Response<bool>> RemoveFromCartAsync(RemoveFromCartRequestDto dto)
        {
            var cart = await EnsureCartExistsAsync(dto.CartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItem = await _cartRepository.GetCartItemAsync(dto.CartItemId);
            if (cartItem == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var removed = await _cartRepository.RemoveCartItemAsync(cartItem);
            if (!removed)
                return _responseHandler.BadRequest<bool>(SystemMessages.SERVER_ERROR);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);
            return _responseHandler.Success(true, SystemMessages.ITEM_REMOVED_FROM_CART);

        }

        public async Task<Response<bool>> DeleteCartAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var deleted = await _cartRepository.DeleteAsync(cart);
            if (!deleted)
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        public async Task<Response<IEnumerable<CartItemResponseDto>>> GetCartItemsAsync(Guid cartId)
        {
            _logger.LogDebug("GetCartItemsAsync called for CartId={CartId}", cartId);

            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null || cart.status == CartStatus.Abandoned)
            {
                _logger.LogWarning("Cart not found for GetCartItemsAsync: {CartId}", cartId);
                return _responseHandler.NotFound<IEnumerable<CartItemResponseDto>>(SystemMessages.NOT_FOUND);
            }

            var items = await _cartRepository.GetCartItemsAsync(cart.Id);
            var dto = _mapper.Map<IEnumerable<CartItemResponseDto>>(items);
            return _responseHandler.Success(dto);
        }

        public async Task<Response<bool>> ClearCartAsync(Guid cartId)
        {
            var cart = await EnsureCartExistsAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.Success(true, SystemMessages.CART_CLEARED);

            foreach (var item in cartItems)
                await _cartRepository.RemoveCartItemAsync(item);
            await _cartRepository.CalculateCartTotalAsync(cart.Id);
            return _responseHandler.Success(true, SystemMessages.CART_CLEARED);

        }

        #endregion

        #region Private Helpers

        private async Task<Cart?> EnsureCartExistsAsync(Guid cartId, bool track = false) =>
            await _cartRepository.GetByIdAsync(cartId, track);

        private async Task<Product?> EnsureProductExistsAsync(Guid productId) =>
            await _productRepository.GetByIdAsync(productId);

        private async Task<Reservation?> EnsureReservationExistsAsync(Guid reservationId, bool track = false) =>
            await _reservationRepository.GetByIdAsync(reservationId, track);

        #endregion
    }
}
