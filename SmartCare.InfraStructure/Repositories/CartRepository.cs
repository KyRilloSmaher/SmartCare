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
        /// Common include chain for loading carts.
        /// </summary>
        private IQueryable<Cart> CartIncludes()
        {
            return _context.Carts
                .Include(c => c.Items
                    .Where(i => i.Reservation.Status == ReservationStatus.ReservedUntilCheckout))
                        .ThenInclude(i => i.Reservation)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images);
        }

        /// <summary>
        /// Gets total available stock for a product.
        /// </summary>
        private async Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            return await _context.Inventories
                .Where(i => i.ProductId == productId)
                .SumAsync(i => i.StockQuantity - i.ReservedQuantity);
        }

        /// <summary>
        /// Updates product availability automatically based on inventory.
        /// </summary>
        private async Task UpdateProductAvailabilityAsync(Guid productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null)
            {
                product.IsAvailable = ((await GetTotalStockForProductAsync(productId)) > 0);
            }
        }

        #endregion

        #region --- Cart Methods ---

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
            return await CartIncludes()
                .Where(c => c.ClientId == userId && c.status == CartStatus.Active)
                .AsNoTracking()
                .FirstOrDefaultAsync();
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
                .Include(ci => ci.Reservation)
                .Where(ci => ci.CartId == cartId &&
                             ci.Reservation.Status == ReservationStatus.ReservedUntilCheckout)
                .ToListAsync();
        }

        public async Task<bool> MarkCartAsCheckedOutAsync(Cart cart)
        {
            cart.status = CartStatus.CheckedOut;
            _context.Carts.Update(cart);
            return true;
        }

        public override async Task<bool> DeleteAsync(Cart entity)
        {
            entity.status = CartStatus.Abandoned;
            _context.Carts.Update(entity);
            return true;
        }

        #endregion

        #region --- CartItem Methods ---

        public async Task<CartItem?> GetCartItemAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Include(ci => ci.Reservation)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<bool> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            await UpdateProductAvailabilityAsync(cartItem.ProductId);
            return true;
        }

        public async Task<bool> UpdateItemCartAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await UpdateProductAvailabilityAsync(cartItem.ProductId);
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(CartItem cartItem)
        {
            // restore inventory reservation
            if (cartItem.InventoryId != Guid.Empty)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(inv => inv.Id == cartItem.InventoryId);

                if (inventory != null)
                {
                    inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - cartItem.Quantity);
                }
            }

            _context.CartItems.Remove(cartItem);
            await UpdateProductAvailabilityAsync(cartItem.ProductId);
            return true;
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var total = await _context.CartItems
                .Include(ci => ci.Reservation)
                .Where(ci => ci.CartId == cartId &&
                             ci.Reservation.Status == ReservationStatus.ReservedUntilCheckout)
                .SumAsync(ci => (decimal)(ci.Quantity * ci.UnitPrice));

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart != null)
            {
                cart.TotalPrice = total;
                _context.Carts.Update(cart);
            }

            return total;
        }

        public async Task<bool> ClearCartAsync(Cart cart)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            // release reserved inventory
            foreach (var item in items)
            {
                if (item.InventoryId != Guid.Empty)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

                    if (inventory != null)
                    {
                        inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - item.Quantity);
                    }
                }

                await UpdateProductAvailabilityAsync(item.ProductId);
            }

            _context.CartItems.RemoveRange(items);
            return true;
        }

        public async Task<bool> CheckIfProductExistInCart(Guid cartId, Guid productId)
        {
            return await _context.CartItems
                .Include(ci => ci.Reservation)
                .AnyAsync(ci =>
                    ci.CartId == cartId &&
                    ci.ProductId == productId &&
                    ci.Reservation.Status == ReservationStatus.ReservedUntilCheckout);
        }

        #endregion
    }
}
