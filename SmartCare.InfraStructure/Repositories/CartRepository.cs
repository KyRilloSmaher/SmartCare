using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class CartRepository : GenericRepository<Cart> ,ICartRepository
    {
        #region Fields
        public readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public CartRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Methods
        public async Task<bool> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);

            await SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart is null)
                  throw new Exception();
            var total = await _context.CartItems
                                      .Where(ci => ci.CartId == cartId)
                                      .SumAsync(ci => (decimal)(ci.Quantity * ci.UnitPrice));
             cart.TotalPrice = total;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return total;
        }

        public async Task<bool> ClearCartAsync(Cart cart)
        {
            var cartItems = await _context.CartItems
                                          .Where(ci => ci.CartId == cart.Id)
                                          .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await SaveChangesAsync();
            return true;
        }

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var newCart = new Cart
            {
                ClientId = userId,
                status = CartStatus.Active
            };

            await _context.Carts.AddAsync(newCart);
            await SaveChangesAsync();
            return newCart;
        }

        public async Task<Cart> GetActiveCartAsync(string userId)
        {
            var cart = await _context.Carts
                                     .Where(c => c.ClientId == userId && c.status == CartStatus.Active)
                                     .FirstOrDefaultAsync();

            return cart!;
        }

        public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId)
        {
            var cartItem = await _context.CartItems
                                         .Where(ci => ci.CartId == cartId && ci.ProductId == productId)
                                         .FirstOrDefaultAsync();

            return cartItem;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            var cartItems = await _context.CartItems
                                          .Where(ci => ci.CartId == cartId)
                                          .ToListAsync();

            return cartItems;
        }

        public async Task<bool> MarkCartAsCheckedOutAsync(Cart cart)
        {
            cart.status = CartStatus.CheckedOut;
            _context.Carts.Update(cart);
            await SaveChangesAsync();
            return true;

        }

        public async Task<bool> RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemCartAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await SaveChangesAsync();
            return true;
        }
        #endregion


    }
}
