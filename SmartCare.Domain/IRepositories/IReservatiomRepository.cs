using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IReservationRepository
    {
        /// <summary>
        /// Creates a reservation for a product in a specific inventory 
        /// </summary>
        Task<Reservation> CreateReservationAsync(
            Guid OrderItemId,
            Guid productId,
            Guid inventoryId,
            int quantity,
            ReservationStatus status = ReservationStatus.ReservedUntilPickup);

        /// <summary>
        /// Cancels a reservation and releases inventory
        /// </summary>
        Task<bool> CancelReservationAsync(
            Guid reservationId,
            Guid inventoryId,
            ReservationStatus status = ReservationStatus.Realesed);

        /// <summary>
        /// Updates reservation status and optionally extends expiration
        /// </summary>
        Task<bool> UpdateReservationStatusAsync(
            Guid reservationId,
            ReservationStatus status,
            int? extendMinutes = null);

        /// <summary>
        /// Releases all reservations for a given order
        /// </summary>
        Task<bool> ReleaseAllReservationsForOrderAsync(Guid orderId);

        /// <summary>
        /// Get reservation by its ID
        /// </summary>
        Task<Reservation?> GetByIdAsync(Guid reservationId, bool tracking = false);

        /// <summary>
        /// Get total reserved quantity for a product
        /// </summary>
        Task<int> GetTotalReservedQuantityForProductAsync(Guid productId);
    }
}
