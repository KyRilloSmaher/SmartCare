using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Cart.Commands;
using SmartCare.Application.CQRs.Cart.Queries;
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
        //private readonly ICartService _cartService;
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //public CartController(ICartService cartService)
        //{
        //    _cartService = cartService;
        //}

        /// <summary>
        /// Get Cart By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Cart.GetById)]
        [ProducesResponseType(typeof(Response<CartResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCartByIdAsync(Guid id)
        {
            //var result = await _cartService.GetCartByIdAsync(id);
            var result = await _mediator.Send(new GetCartByIdAsyncQuery(id));
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
            //var result = await _cartService.GetUserActiveCartAsync(userId);
            var result = await _mediator.Send(new GetUserActiveCartAsyncQuery(userId));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Items in a Cart
        /// </summary>
        [HttpGet(ApplicationRouting.Cart.GetById + "/items")]
        [ProducesResponseType(typeof(Response<IEnumerable<CartItemResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCartItemsAsync(Guid id)
        {
            //var result = await _cartService.GetCartItemsAsync(id);
            var result = await _mediator.Send(new GetCartItemsAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Add Item to Cart
        /// </summary>
        [HttpPost(ApplicationRouting.Cart.Create)]
        [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUserCart()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _cartService.CreateCartForUserAsync(userId);
            var result  = await _mediator.Send(new CreateCartForUserAsyncCommand(userId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Add Item to Cart
        /// </summary>
        [HttpPost(ApplicationRouting.Cart.AddItem)]
        [ProducesResponseType(typeof(Response<CartItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddToCartAsync([FromBody] AddToCartRequestDto dto)
        {
            //var result = await _cartService.AddToCartAsync(dto);
            var result = await _mediator.Send(new AddToCartAsyncCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update Cart Item Quantity
        /// </summary>
        [HttpPut(ApplicationRouting.Cart.UpdateItem)]
        [ProducesResponseType(typeof(Response<CartItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCartItemAsync([FromBody] UpdateCartItemRequestDto dto)
        {
            //var result = await _cartService.UpdateCartItemQuantityAsync(dto);
            var result = await _mediator.Send(new UpdateCartItemQuantityAsyncCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Remove Item from Cart
        /// </summary>
        [HttpDelete(ApplicationRouting.Cart.RemoveItem)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFromCartAsync([FromBody] RemoveFromCartRequestDto dto)
        {
            //var result = await _cartService.RemoveFromCartAsync(dto);
            var result = await _mediator.Send(new RemoveFromCartAsyncCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Clear All Items in Cart
        /// </summary>
        [HttpDelete(ApplicationRouting.Cart.Clear)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCartAsync([FromRoute] Guid id)
        {
            //var result = await _cartService.ClearCartAsync(id);
            var result = await _mediator.Send(new ClearCartAsyncCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }



        /// <summary>
        /// Delete Entire Cart
        /// </summary>
        //[Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Cart.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCartAsync(Guid cartId)
        {
            //var result = await _cartService.DeleteCartAsync(cartId);
            var result = await _mediator.Send(new DeleteCartAsyncCommand(cartId));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
