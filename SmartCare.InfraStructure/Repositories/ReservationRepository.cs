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
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        private readonly ApplicationDBContext _context;

        public ReservationRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #region Methods

        /// <summary>
        /// Creates a new reservation for a product in a cart.
        /// </summary>
        public async Task<Reservation?> CreateReservationAsync(CartItem cartItem, int quantity, ReservationStatus status)
        {
            if (cartItem.InventoryId == Guid.Empty)
                return null;

            var inventory = await _context.Inventories.FirstOrDefaultAsync(inv => inv.Id == cartItem.InventoryId);
            if (inventory == null)
                return null;

            if (inventory.StockQuantity - inventory.ReservedQuantity < quantity)
                return null;

            inventory.ReservedQuantity += quantity;

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                CartItemId = cartItem.CartItemId,
                QuantityReserved = quantity,
                ReservedAt = DateTime.UtcNow,
                Status = status,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        /// <summary>
        /// Cancels a reservation and releases inventory.
        /// </summary>
        public async Task<bool> CancelReservationAsync(Guid reservationId, Guid inventoryId, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);
            if (reservation == null)
                return false;

            var inventory = await _context.Inventories.FirstOrDefaultAsync(inv => inv.Id == inventoryId);
            if (inventory == null)
                return false;

            inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - reservation.QuantityReserved);

            reservation.Status = status;
            reservation.ExpiredAt = DateTime.UtcNow;

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ReservationId == reservationId);
            if (cartItem != null)
                _context.CartItems.Remove(cartItem);

            _context.Reservations.Update(reservation);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates the quantity of a reservation.
        /// </summary>
        public async Task<bool> UpdateReservationQuantityAsync(Reservation reservation, int newQuantity)
        {
            if (reservation == null || newQuantity < 0)
                return false;

            var cartItem = await _context.CartItems
                .Include(ci => ci.Inventory)
                .FirstOrDefaultAsync(ci => ci.CartItemId == reservation.CartItemId);

            if (cartItem?.Inventory == null)
                return false;

            var inventory = cartItem.Inventory;
            int quantityDifference = newQuantity - reservation.QuantityReserved;

            if (quantityDifference > 0 && (inventory.StockQuantity - inventory.ReservedQuantity < quantityDifference))
                return false;

            inventory.ReservedQuantity += quantityDifference;
            reservation.QuantityReserved = newQuantity;
            reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(10);

            _context.Reservations.Update(reservation);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Releases all reservations for a cart.
        /// </summary>
        public async Task<bool> ReleaseAllReservationsForCartAsync(Guid cartId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Reservation)
                .Include(ci => ci.Inventory)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Reservation == null || cartItem.Reservation.Status == ReservationStatus.Realesed)
                    continue;

                if (cartItem.Inventory != null)
                    cartItem.Inventory.ReservedQuantity = Math.Max(0, cartItem.Inventory.ReservedQuantity - cartItem.Reservation.QuantityReserved);

                cartItem.Reservation.Status = ReservationStatus.Realesed;
                cartItem.Reservation.ExpiredAt = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Removes all expired reservations.
        /// </summary>
        public async Task<int> RemoveAllExpiredReservationsAsync()
        {
            var expiredReservations = await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .Where(r => r.ExpiredAt < DateTime.UtcNow && r.Status != ReservationStatus.Realesed)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                if (reservation.CartItem?.Inventory != null)
                    reservation.CartItem.Inventory.ReservedQuantity = Math.Max(0,
                        reservation.CartItem.Inventory.ReservedQuantity - reservation.QuantityReserved);
            }

            _context.Reservations.RemoveRange(expiredReservations);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets all active reservations for a product.
        /// </summary>
        public async Task<IEnumerable<Reservation>> GetActiveReservationsByProductAsync(Guid productId)
        {
            return await _context.Reservations
                .Include(r => r.CartItem)
                .ThenInclude(ci => ci.Inventory)
                .Where(r => r.CartItem.Inventory.ProductId == productId &&
                            r.ExpiredAt >= DateTime.UtcNow &&
                            (r.Status == ReservationStatus.ReservedUntilPayment || r.Status == ReservationStatus.ReservedUntilCheckout))
                .OrderBy(r => r.ReservedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a reservation by its cart item ID.
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
        /// Gets reservations that are about to expire.
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
        /// Extends a reservation's expiration.
        /// </summary>
        public async Task<bool> ExtendReservationAsync(Guid reservationId, int additionalMinutes)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null || reservation.Status == ReservationStatus.Realesed)
                return false;

            reservation.ExpiredAt = reservation.ExpiredAt.AddMinutes(additionalMinutes);
            reservation.Status = ReservationStatus.ReservedUntilCheckout;
            _context.Reservations.Update(reservation);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Gets total reserved quantity for a product.
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
