using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get Cart By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Cart.GetById)]
        [ProducesResponseType(typeof(Response<CartResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCartByIdAsync(Guid id)
        {
            var result = await _cartService.GetCartByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Active Cart for the Logged-in User
        /// </summary>
        [HttpGet(ApplicationRouting.Cart.GetForUser)]
        [ProducesResponseType(typeof(Response<CartResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserActiveCartAsync()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _cartService.GetUserActiveCartAsync(userId);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Items in a Cart
        /// </summary>
        [HttpGet(ApplicationRouting.Cart.GetById + "/items")]
        [ProducesResponseType(typeof(Response<IEnumerable<CartItemResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCartItemsAsync(Guid id)
        {
            var result = await _cartService.GetCartItemsAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Add Item to Cart
        /// </summary>
        [HttpPost(ApplicationRouting.Cart.AddItem)]
        [ProducesResponseType(typeof(Response<CartItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddToCartAsync([FromBody] AddToCartRequestDto dto)
        {
            var result = await _cartService.AddToCartAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update Cart Item Quantity
        /// </summary>
        [HttpPut(ApplicationRouting.Cart.UpdateItem)]
        [ProducesResponseType(typeof(Response<CartItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCartItemAsync([FromBody] UpdateCartItemRequestDto dto)
        {
            var result = await _cartService.UpdateCartItemQuantityAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Remove Item from Cart
        /// </summary>
        [HttpDelete(ApplicationRouting.Cart.RemoveItem)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFromCartAsync([FromBody] RemoveFromCartRequestDto dto)
        {
            var result = await _cartService.RemoveFromCartAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Clear All Items in Cart
        /// </summary>
        [HttpDelete(ApplicationRouting.Cart.Clear)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCartAsync([FromQuery] Guid cartId)
        {
            var result = await _cartService.ClearCartAsync(cartId);
            return ControllersHelperMethods.FinalResponse(result);
        }



        /// <summary>
        /// Delete Entire Cart
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Cart.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCartAsync(Guid cartId)
        {
            var result = await _cartService.DeleteCartAsync(cartId);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
