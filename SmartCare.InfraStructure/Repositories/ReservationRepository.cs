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
        public async Task<Reservation?> CreateReservationAsync(CartItem CartItem, int quantity, ReservationStatus status)
        {

            if (CartItem.InventoryId == Guid.Empty)
                    return null;

            var inventory = await _context.Inventories.FirstOrDefaultAsync(inv => inv.Id == CartItem.InventoryId);
            if (inventory == null) return null;

                // Check if there's enough available stock (total stock minus already reserved)
                if (inventory.StockQuantity - inventory.ReservedQuantity < quantity)
                    return null;

                inventory.ReservedQuantity += quantity;

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    CartItemId = CartItem.CartItemId,
                    QuantityReserved = quantity,
                    ReservedAt = DateTime.UtcNow,
                    Status = status,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(10),
                };

                await _context.Reservations.AddAsync(reservation);
                return reservation;
        }
        

        /// <summary>
        /// Cancels (removes) a reservation.
        /// </summary>
        public async Task<bool> CancelReservationAsync(Reservation reservation, ReservationStatus status)
        {
            if (reservation == null)
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Inventory)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == reservation.CartItemId);

                if (cartItem?.Inventory == null)
                    return false;

                var inventory = cartItem.Inventory;
                inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - reservation.QuantityReserved);

                reservation.Status = status;
                reservation.ExpiredAt = DateTime.UtcNow; // Mark as expired immediately

                _context.Reservations.Update(reservation);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Updates the quantity of a reservation.
        /// </summary>
        public async Task<bool> UpdateReservationQuantityAsync(Reservation reservation, int newQuantity)
        {
            if (reservation == null || newQuantity < 0)
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Inventory)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == reservation.CartItemId);

                if (cartItem?.Inventory == null)
                    return false;

                var inventory = cartItem.Inventory;
                int quantityDifference = newQuantity - reservation.QuantityReserved;

                // Check if there's enough available stock for the increase
                if (quantityDifference > 0 && (inventory.StockQuantity - inventory.ReservedQuantity < quantityDifference))
                    return false;

                inventory.ReservedQuantity += quantityDifference;
                reservation.QuantityReserved = newQuantity;
                reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(10); // Reset expiration

                _context.Reservations.Update(reservation);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Removes all reservations related to a given cart.
        /// </summary>
        public async Task<bool> ReleaseAllReservationsForCartAsync(Guid cartId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get all cart items for the cart with their reservations and inventory
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Reservation)
                    .Include(ci => ci.Inventory)
                    .Where(ci => ci.CartId == cartId)
                    .ToListAsync();

                foreach (var cartItem in cartItems)
                {
                  
                        if (cartItem.Reservation.Status != ReservationStatus.Realesed)
                        {
                            // Release reserved quantity back to inventory
                            if (cartItem.Inventory != null)
                            {
                                cartItem.Inventory.ReservedQuantity = Math.Max(0,cartItem.Inventory.ReservedQuantity - cartItem.Reservation.QuantityReserved);
                            }

                            cartItem.Reservation.Status = ReservationStatus.Realesed;
                            cartItem.Reservation.ExpiredAt = DateTime.UtcNow;
                        }
                    
                }

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Deletes all expired reservations.
        /// </summary>
        public async Task<int> RemoveAllExpiredReservationsAsync()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var expiredReservations = await _context.Reservations
                    .Include(r => r.CartItem)
                    .ThenInclude(ci => ci.Inventory)
                    .Where(r => r.ExpiredAt < DateTime.UtcNow && r.Status != ReservationStatus.Realesed)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    // Release reserved quantity back to inventory
                    if (reservation.CartItem?.Inventory != null)
                    {
                        reservation.CartItem.Inventory.ReservedQuantity = Math.Max(0,
                            reservation.CartItem.Inventory.ReservedQuantity - reservation.QuantityReserved);
                    }
                }

                _context.Reservations.RemoveRange(expiredReservations);
                var removedCount = await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return removedCount;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Gets all active (non-expired) reservations for a product.
        /// </summary>
        public async Task<IEnumerable<Reservation>> GetActiveReservationsByProductAsync(Guid productId)
        {
            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .Where(r => r.CartItem.Inventory.ProductId == productId &&
                           r.ExpiredAt >= DateTime.UtcNow && (r.Status == ReservationStatus.ReservedUntilPayment || r.Status == ReservationStatus.ReservedUntilCheckout))
                .OrderBy(r => r.ReservedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a reservation by its related CartItem ID.
        /// </summary>
        public async Task<Reservation?> GetReservationByCartItemIdAsync(Guid cartItemId)
        {
            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .FirstOrDefaultAsync(r => r.CartItemId == cartItemId &&
                                         r.ExpiredAt >= DateTime.UtcNow &&
                                         r.Status == ReservationStatus.ReservedUntilCheckout);
        }



        /// <summary>
        /// Gets all reservations that are about to expire (within the next few minutes).
        /// </summary>
        public async Task<IEnumerable<Reservation>> GetExpiringReservationsAsync(int minutesUntilExpiration = 5)
        {
            var threshold = DateTime.UtcNow.AddMinutes(minutesUntilExpiration);

            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .Where(r => r.ExpiredAt <= threshold &&
                           r.ExpiredAt > DateTime.UtcNow &&
                           r.Status == ReservationStatus.ReservedUntilCheckout)
                .ToListAsync();
        }

        /// <summary>
        /// Extends the expiration time of a reservation.
        /// </summary>
        public async Task<bool> ExtendReservationAsync(Guid reservationId, int additionalMinutes)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null || reservation.Status != ReservationStatus.Realesed)
                return false;

            reservation.ExpiredAt = reservation.ExpiredAt.AddMinutes(additionalMinutes);
            reservation.Status = ReservationStatus.ReservedUntilCheckout;
            _context.Reservations.Update(reservation);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Gets the total reserved quantity for a specific product.
        /// </summary>
        public async Task<int> GetTotalReservedQuantityForProductAsync(Guid productId)
        {
            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .Where(r => r.CartItem.Inventory.ProductId == productId &&
                           r.ExpiredAt >= DateTime.UtcNow &&
                           (r.Status == ReservationStatus.ReservedUntilCheckout || r.Status == ReservationStatus.ReservedUntilPayment))
                .SumAsync(r => r.QuantityReserved);
        }

        #endregion
    }
}