using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public ReservationRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new reservation for a product in a cart.
        /// </summary>
        public async Task<Reservation?> CreateReservationAsync(Guid CartItemId, int quantity)
        {
            var inventroyId = await _context.CartItems
                .Where(ci => ci.CartItemId == CartItemId)
                .Select(ci => ci.InventoryId)
                .FirstOrDefaultAsync();
            if (inventroyId == Guid.Empty)
                return null;

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventroyId);
            if (inventory == null || inventory.StockQuantity < quantity)
                return null;
            inventory.StockQuantity -= quantity;

            var reservation = new Reservation
            {
                CartItemId = CartItemId,
                QuantityReserved = quantity,
                ReservedAt = DateTime.UtcNow,
                Status = ReservationStatus.ReservedUntilCheckout,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10),
            };

            await _context.Reservations.AddAsync(reservation);
            var saved = await _context.SaveChangesAsync() > 0;
            return saved ? reservation : null;
        }

        /// <summary>
        /// Cancels (removes) a reservation.
        /// </summary>
        public async Task<bool> CancelReservationAsync(Reservation reservation)
        {
            if (reservation == null)
                return false;
            var inventroyId = await _context.CartItems
                .Where(ci => ci.CartItemId == reservation.CartItemId)
                .Select(ci => ci.InventoryId)
                .FirstOrDefaultAsync();
            if (inventroyId == Guid.Empty)
                return false;
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventroyId);
            if (inventory == null)
                return false;
            inventory.StockQuantity += reservation.QuantityReserved;
            _context.Reservations.Remove(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates the quantity of a reservation.
        /// </summary>
        public async Task<bool> UpdateReservationQuantityAsync(Reservation reservation, int newQuantity)
        {
            if (reservation == null)
                return false;

            reservation.QuantityReserved = newQuantity;
            reservation.ReservedAt = DateTime.UtcNow;
            reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(10);

            _context.Reservations.Update(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Removes all reservations related to a given cart.
        /// </summary>
        public async Task<bool> ReleaseAllReservationsForCartAsync(Guid cartId)
        {
            var releasedReservations = await _context.Reservations
                .Include(r => r.CartItem)
                .Where(r => r.CartItem.CartId == cartId)
                .ToListAsync();

            if (!releasedReservations.Any())
                return true;

            _context.Reservations.RemoveRange(releasedReservations);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes all expired reservations.
        /// </summary>
        public async Task<bool> RemoveAllExpiredReservationsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredReservations = await _context.Reservations
                .Where(r => r.ExpiredAt <= now)
                .ToListAsync();

            if (!expiredReservations.Any())
                return true;

            _context.Reservations.RemoveRange(expiredReservations);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all active (non-expired) reservations for a product.
        /// </summary>
        public async Task<IEnumerable<Reservation>> GetActiveReservationsByProductAsync(Guid productId)
        {
            var now = DateTime.UtcNow;
            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Product)
                .Where(r => r.CartItem.ProductId == productId && r.ExpiredAt >= now)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a reservation by its related CartItem ID.
        /// </summary>
        public async Task<Reservation?> GetReservationByCartItemIdAsync(Guid cartItemId)
        {
            return await _context.Reservations
                .Include(r => r.CartItem)
                .FirstOrDefaultAsync(r => r.CartItemId == cartItemId);
        }

        #endregion
    }
}
