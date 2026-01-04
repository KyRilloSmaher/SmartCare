using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly ApplicationDBContext _context;

        public CartRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #region --- Private Helpers ---

        /// <summary>
        /// Common include chain for loading carts with items and product images.
        /// </summary>
        private IQueryable<Cart> CartIncludes()
        {
            return _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images);
        }

        #endregion

        #region --- Cart Methods ---

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var cart = new Cart
            {
                ClientId = userId,
                status = CartStatus.Active
            };

            await AddAsync(cart);
            return cart;
        }

        public async Task<Cart?> GetActiveCartAsync(string userId)
        {
            return await CartIncludes()
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.ClientId == userId &&
                    c.status == CartStatus.Active);
        }

        public override async Task<Cart?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var query = CartIncludes().Where(c => c.Id == id);

            if (!asTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }

        public async Task<bool> MarkCartAsCheckedOutAsync(Cart cart)
        {
            cart.status = CartStatus.CheckedOut;
            await UpdateAsync(cart);
            return true;
        }

        public override async Task<bool> DeleteAsync(Cart cart)
        {
            cart.status = CartStatus.Abandoned;
            await UpdateAsync(cart);
            return true;
        }

        #endregion

        #region --- CartItem Methods ---

        public async Task<CartItem?> GetCartItemAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<bool> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateItemCartAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var total = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => ci.Quantity * ci.UnitPrice);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart != null)
            {
                cart.TotalPrice = total;
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();
            }

            return total;
        }

        public async Task<bool> ClearCartAsync(Cart cart)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckIfProductExistInCart(Guid cartId, Guid productId)
        {
            return await _context.CartItems.AnyAsync(ci =>
                ci.CartId == cartId &&
                ci.ProductId == productId);
        }

        #endregion
    }
}
