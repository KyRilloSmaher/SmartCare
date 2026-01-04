

using SmartCare.Domain.Entities;


namespace SmartCare.Domain.IRepositories
{
    /// <summary>
    /// Provides data access operations for carts and cart items.
    /// Handles only cart state and item management (no stock or inventory logic).
    /// </summary>
    public interface ICartRepository
    {
        #region --- Cart ---

        /// <summary>
        /// Creates a new active cart for a specific user.
        /// </summary>
        Task<Cart> CreateCartAsync(string userId);

        /// <summary>
        /// Gets the active cart for a user if exists.
        /// </summary>
        Task<Cart?> GetActiveCartAsync(string userId);

        /// <summary>
        /// Gets a cart by its identifier.
        /// </summary>
        Task<Cart?> GetByIdAsync(Guid cartId, bool asTracking = false);

        /// <summary>
        /// Marks the cart as checked out.
        /// </summary>
        Task<bool> MarkCartAsCheckedOutAsync(Cart cart);

        /// <summary>
        /// Soft deletes the cart by marking it as abandoned.
        /// </summary>
        Task<bool> DeleteAsync(Cart cart);

        #endregion

        #region --- Cart Items ---

        /// <summary>
        /// Gets all items belonging to a cart.
        /// </summary>
        Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId);

        /// <summary>
        /// Gets a single cart item by its identifier.
        /// </summary>
        Task<CartItem?> GetCartItemAsync(Guid cartItemId);

        /// <summary>
        /// Adds a new item to the cart.
        /// </summary>
        Task<bool> AddCartItemAsync(CartItem cartItem);

        /// <summary>
        /// Updates an existing cart item (e.g. quantity or price).
        /// </summary>
        Task<bool> UpdateItemCartAsync(CartItem cartItem);

        /// <summary>
        /// Removes a cart item.
        /// </summary>
        Task<bool> RemoveCartItemAsync(CartItem cartItem);

        /// <summary>
        /// Removes all items from the cart.
        /// </summary>
        Task<bool> ClearCartAsync(Cart cart);

        /// <summary>
        /// Checks if a product already exists in the cart.
        /// </summary>
        Task<bool> CheckIfProductExistInCart(Guid cartId, Guid productId);

        /// <summary>
        /// Calculates and updates the total price of the cart.
        /// </summary>
        Task<decimal> CalculateCartTotalAsync(Guid cartId);

        #endregion
    }
}
