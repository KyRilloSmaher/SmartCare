using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface ICartService
    {
        Task<Response<CartResponseDto?>> GetCartByIdAsync(Guid cartId);
        Task<Response<CartResponseDto>> GetUserActiveCartAsync(string userId);
        Task<Response<Guid>> CreateCartForUserAsync(string userId);
        Task<Response<CartItemResponseDto?>> AddToCartAsync(AddToCartRequestDto dto);
        Task<Response<bool>> RemoveFromCartAsync(RemoveFromCartRequestDto dto);
        Task<Response<CartItemResponseDto?>> UpdateCartItemQuantityAsync(UpdateCartItemRequestDto dto);
        Task<Response<bool>> ClearCartAsync(Guid cartId);
        Task<Response<IEnumerable<CartItemResponseDto>>> GetCartItemsAsync(Guid cartId);
        Task<Response<bool>> DeleteCartAsync(Guid cartId);
    }
}
