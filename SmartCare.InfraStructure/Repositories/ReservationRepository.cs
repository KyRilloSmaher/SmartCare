using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Exceptions;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly int _defaultReservationDayForPickUp;
        private readonly int _defaultReservationDayForOnlinepayment;

        public ReservationRepository(ApplicationDBContext context, IConfiguration configuration)
            : base(context)
        {
            _context = context;
            _defaultReservationDayForPickUp = configuration.GetValue<int>("ReservationTimes:DaysForPickUp");
            _defaultReservationDayForOnlinepayment = configuration.GetValue<int>("ReservationTimes:HoursForPayment");
        }

        #region Query Methods

        public IQueryable<Reservation> GetReservationsQueryable(bool trackChanges = false)
        {
            var query = _context.Reservations
                .Include(r => r.product)
                .Include(r => r.inventory)
                .Include(r => r.OrderItem)
                .AsQueryable();

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<Reservation?> GetByIdAsync(Guid reservationId, bool trackChanges = false)
        {
            var query = _context.Reservations
                 .Include(r => r.product)
                .Include(r => r.inventory)
                .Include(r => r.OrderItem)
                .Where(r => r.Id == reservationId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalReservedQuantityForProductAsync(Guid productId)
        {
            return await _context.Reservations
                .Where(r => r.ProductId == productId && r.Status != ReservationStatus.Realesed)
                .SumAsync(r => r.QuantityReserved);
        }

        #endregion

        #region Business Logic Methods

        public async Task<Reservation> CreateReservationAsync(Guid orderItemId, Guid productId, Guid inventoryId, int quantity,
            ReservationStatus status = ReservationStatus.ReservedUntilPickup)
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
                ExpiredAt = status == ReservationStatus.ReservedUntilPickup
                    ? DateTime.UtcNow.AddDays(_defaultReservationDayForPickUp)
                    : DateTime.UtcNow.AddHours(_defaultReservationDayForOnlinepayment),
                OrderItemId = orderItemId
            };

            await _context.Reservations.AddAsync(reservation);
            return reservation;
        }

        public async Task<bool> CancelReservationAsync(Guid reservationId, Guid inventoryId,
            ReservationStatus status = ReservationStatus.Realesed)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return false;

            if (reservation.Status == ReservationStatus.Realesed ||
                reservation.Status == ReservationStatus.Completed)
                return false;

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory != null)
            {
                inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - reservation.QuantityReserved);
            }

            reservation.Status = status;
            reservation.ExpiredAt = DateTime.UtcNow;

            return true;
        }

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

            return true;
        }

        public async Task<bool> ReleaseAllReservationsForOrderAsync(Guid orderId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.OrderItem.OrderId == orderId && r.Status != ReservationStatus.Realesed)
                .ToListAsync();

            foreach (var r in reservations)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == r.InventoryId);

                if (inventory != null)
                    inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - r.QuantityReserved);

                r.Status = ReservationStatus.Realesed;
                r.ExpiredAt = DateTime.UtcNow;
            }

            return true;
        }

        public async Task<Reservation?> CreateReservationAsync(Guid OrderItemId, Guid productId, Guid inventoryId, int quantity, DateTime ExpiredAt, ReservationStatus status = ReservationStatus.ReservedUntilPickup)
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
                ExpiredAt = ExpiredAt,
                OrderItemId = OrderItemId
            };

            await _context.Reservations.AddAsync(reservation);
            return reservation;
        }

        public async Task<bool> Delete(Guid Id)
        {
           var reservation =  await _context.Reservations.FirstOrDefaultAsync(r => r.Id == Id);
            if (reservation is null)
            {
                throw new DomainException("Can not Delete Reservation because it does not exist");
            }
            _context.Reservations.Remove(reservation);
            return true;
        }

        #endregion
    }
}