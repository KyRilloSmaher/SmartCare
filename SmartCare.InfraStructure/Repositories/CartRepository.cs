using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly ApplicationDBContext _context;

        public CartRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #region Private Helpers

        private IQueryable<Cart> CartIncludes(bool trackChanges = false)
        {
            var query = _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images);

            return trackChanges ? query : query.AsNoTracking();
        }

        #endregion

        #region Cart Methods

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
<<<<<<< HEAD
        public async Task<Cart?> GetActiveCartAsync(string userId, bool track = false)
        {
            var query = CartIncludes()
                .Where(c => c.ClientId == userId && c.status == CartStatus.Active);

            if (!track)
                query = query.AsNoTracking();

=======

        public async Task<Cart?> GetActiveCartAsync(string userId, bool trackChanges = false)
        {
            var query = CartIncludes(trackChanges)
                .Where(c => c.ClientId == userId && c.status == CartStatus.Active);

>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
            return await query.FirstOrDefaultAsync();
        }


        public override async Task<Cart?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            return await CartIncludes(asTracking)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Where(ci => ci.CartId == cartId)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task MarkCartAsCheckedOutAsync(Cart cart)
        {
            cart.status = CartStatus.CheckedOut;
            return Task.CompletedTask;
        }

        public override Task DeleteAsync(Cart cart)
        {
            if (cart == null) return false;
            cart.status = CartStatus.Abandoned;
<<<<<<< HEAD

            _dbContext.Set<Cart>().Update(cart);
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
=======
            return Task.CompletedTask;
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var total = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => ci.Quantity * ci.UnitPrice);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart != null)
                cart.TotalPrice = total;

            return total;
        }

        public async Task<bool> CheckIfProductExistInCart(Guid cartId, Guid productId)
        {
            return await _context.CartItems
                .AnyAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }

        #endregion

        #region CartItem Methods

        public async Task<CartItem?> GetCartItemAsync(Guid cartItemId, bool trackChanges = false)
        {
            var query = _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Where(ci => ci.CartItemId == cartItemId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            cartItem.Cart.ReCalculateTotalPrice();
        }

        public Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            return Task.CompletedTask;
        }

        public async Task<bool> ClearCartAsync(Guid cartId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            var cart = await _context.Carts.FirstOrDefaultAsync(c=>c.Id == cartId);
            cart.TotalPrice = 0;
            return true;
        }

        #endregion
    }
}