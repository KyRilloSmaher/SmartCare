using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly int _defaultReservationDayForPickUp;
        private readonly int _defaultReservationDayForOnlinepayment;

        public ReservationRepository(ApplicationDBContext context, IConfiguration configuration)
            : base(context)
        {
            _context = context;
            _configuration = configuration;
            _defaultReservationDayForPickUp = _configuration.GetValue<int>("ReservationTimes:DaysForPickUp");
            _defaultReservationDayForOnlinepayment = _configuration.GetValue<int>("ReservationTimes:HoursForPayment");
        }

        #region Reservation Methods

        /// <summary>
        /// Creates a reservation for a product in a specific inventory
        /// Used by Pickup Orders
        /// </summary>
        public async Task<Reservation> CreateReservationAsync(Guid OrderItemId ,Guid productId,Guid inventoryId,int quantity, ReservationStatus status = ReservationStatus.ReservedUntilPickup)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null || inventory.StockQuantity - inventory.ReservedQuantity < quantity)
                throw new InvalidOperationException("Not enough stock to reserve");

            inventory.ReservedQuantity += quantity;
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                InventoryId = inventoryId,
                QuantityReserved = quantity,
                ReservedAt = DateTime.UtcNow,
                Status = status,
                ExpiredAt = status == ReservationStatus.ReservedUntilPickup ? DateTime.UtcNow.AddDays(_defaultReservationDayForPickUp) : DateTime.UtcNow.AddHours(_defaultReservationDayForOnlinepayment),
                OrderItemId = OrderItemId
            };

            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        /// <summary>
        /// Cancels a reservation and releases inventory
        /// </summary>
        public async Task<bool> CancelReservationAsync(Guid reservationId,Guid inventoryId, ReservationStatus status = ReservationStatus.Realesed)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return false;
            if (reservation.Status == ReservationStatus.Realesed || reservation.Status == ReservationStatus.Completed)
                  return false;
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory != null)
            {
                inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - reservation.QuantityReserved);
            }

            reservation.Status = status;
            reservation.ExpiredAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates reservation status and expiry
        /// </summary>
        public async Task<bool> UpdateReservationStatusAsync(Guid reservationId, ReservationStatus status, int? extendMinutes = null)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return false;
                
            reservation.Status = status;
            if (reservation.Status == ReservationStatus.Completed)
                 reservation.ExpiredAt = DateTime.UtcNow;
            if (extendMinutes.HasValue && extendMinutes > 0)
                reservation.ExpiredAt = reservation.ExpiredAt.AddMinutes(extendMinutes.Value);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Releases all reservations for a given order
        /// </summary>
        public async Task<bool> ReleaseAllReservationsForOrderAsync(Guid orderId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.OrderItem.OrderId == orderId && r.Status != ReservationStatus.Realesed)
                .ToListAsync();

            foreach (var r in reservations)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == r.InventoryId);
                if (inventory != null)
                    inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - r.QuantityReserved);

                r.Status = ReservationStatus.Realesed;
                r.ExpiredAt = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Gets a reservation by ID
        /// </summary>
        public async Task<Reservation?> GetByIdAsync(Guid reservationId, bool tracking = false)
        {
            IQueryable<Reservation> query = _context.Reservations;
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        /// <summary>
        /// Gets total reserved quantity for a product
        /// </summary>
        public async Task<int> GetTotalReservedQuantityForProductAsync(Guid productId)
        {
            return await _context.Reservations
                .Where(r => r.ProductId == productId && r.Status != ReservationStatus.Realesed)
                .SumAsync(r => r.QuantityReserved);
        }

        #endregion
    }
}
