using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetActiveCartAsync(string userId);
        Task<bool> AddCartItemAsync(CartItem cartItem);
        Task<bool> UpdateItemCartAsync(CartItem cartItem);
        Task<bool> RemoveCartItemAsync(CartItem cartItem);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId);
        Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId);
        Task<bool> MarkCartAsCheckedOutAsync(Cart cart);
        Task<Cart> CreateCartAsync(string userId);
        Task<bool> ClearCartAsync(Cart cart);
    }
}
