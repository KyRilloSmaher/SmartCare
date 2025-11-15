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

        #region Cart Methods

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var newCart = new Cart
            {
                ClientId = userId,
                status = CartStatus.Active
            };

            await _context.Carts.AddAsync(newCart);
            return newCart;
        }

        public async Task<Cart?> GetActiveCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Reservation)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .Where(c => c.ClientId == userId && c.status == CartStatus.Active)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (cart != null)
                cart.Items = cart.Items
                    .Where(i => i.Reservation.Status == ReservationStatus.ReservedUntilCheckout)
                    .ToList();

            return cart;
        }

        public async override Task<Cart?> GetByIdAsync(Guid Id, bool AsTracking = false)
        {
            var query = _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Reservation)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .Where(c => c.Id == Id);

            if (!AsTracking)
                query = query.AsNoTracking();

            var cart = await query.FirstOrDefaultAsync();

            if (cart != null)
                cart.Items = cart.Items
                    .Where(i => i.Reservation.Status == ReservationStatus.ReservedUntilCheckout)
                    .ToList();

            return cart;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Include(ci => ci.Reservation)
                .Where(ci => ci.CartId == cartId && ci.Reservation.Status == ReservationStatus.ReservedUntilCheckout)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .AsTracking()
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<bool> MarkCartAsCheckedOutAsync(Cart cart)
        {
            cart.status = CartStatus.CheckedOut;
            _context.Carts.Update(cart);
            return true;
        }

        #endregion

        #region CartItem Methods

        public async Task<bool> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            return true;
        }

        public async Task<bool> UpdateItemCartAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(CartItem cartItem)
        {
            if (cartItem.InventoryId != Guid.Empty)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(inv => inv.Id == cartItem.InventoryId);

                if (inventory != null)
                    inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - cartItem.Quantity);
            }

            _context.CartItems.Remove(cartItem);
            return true;
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var cart = await _context.Carts
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
                throw new Exception($"Cart {cartId} not found.");

            var total = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => (decimal)(ci.Quantity * ci.UnitPrice));

            cart.TotalPrice = total;
            _context.Carts.Update(cart);

            return total;
        }

        public async Task<bool> ClearCartAsync(Cart cart)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            return true;
        }

        #endregion
    }
}
