

using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;


namespace SmartCare.Domain.IRepositories
{
    /// <summary>
    /// Provides data access operations for carts and cart items.
    /// Handles only cart state and item management (no stock or inventory logic).
    /// </summary>
    public interface ICartRepository
    {

        Task<Cart> CreateCartAsync(string userId);

        Task<Cart?> GetActiveCartAsync(string userId, bool trackChanges = false);

        Task<Cart?> GetByIdAsync(Guid id, bool asTracking = false);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId);

        Task MarkCartAsCheckedOutAsync(Cart cart);
        Task DeleteAsync(Cart cart);
        Task<decimal> CalculateCartTotalAsync(Guid cartId);

        Task<bool> CheckIfProductExistInCart(Guid cartId, Guid productId);
        Task<CartItem?> GetCartItemAsync(Guid cartItemId, bool trackChanges = false);

        Task AddCartItemAsync(CartItem cartItem);

        Task RemoveCartItemAsync(CartItem cartItem);

        Task<bool> ClearCartAsync(Guid cartId);
    }
}