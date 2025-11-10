using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{

    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        /// <summary>
        /// Creates a new reservation for a product in a cart.
        /// </summary>
        Task<Reservation?> CreateReservationAsync(CartItem CartItem, int quantity, ReservationStatus status);

        /// <summary>
        /// Cancels (removes) a reservation.
        /// </summary>
        Task<bool> CancelReservationAsync(Reservation reservation, ReservationStatus status);

        /// <summary>
        /// Updates the quantity of a reservation.
        /// </summary>
        Task<bool> UpdateReservationQuantityAsync(Reservation reservation, int newQuantity);

        /// <summary>
        /// Removes all reservations related to a given cart.
        /// </summary>
        Task<bool> ReleaseAllReservationsForCartAsync(Guid cartId);

        /// <summary>
        /// Deletes all expired reservations.
        /// </summary>
        Task<int> RemoveAllExpiredReservationsAsync();

        /// <summary>
        /// Gets all active (non-expired) reservations for a product.
        /// </summary>
        Task<IEnumerable<Reservation>> GetActiveReservationsByProductAsync(Guid productId);

        /// <summary>
        /// Gets a reservation by its related CartItem ID.
        /// </summary>
        Task<Reservation?> GetReservationByCartItemIdAsync(Guid cartItemId);


        /// <summary>
        /// Gets all reservations that are about to expire (within the next few minutes).
        /// </summary>
        Task<IEnumerable<Reservation>> GetExpiringReservationsAsync(int minutesUntilExpiration = 5);

        /// <summary>
        /// Extends the expiration time of a reservation.
        /// </summary>
        Task<bool> ExtendReservationAsync(Guid reservationId, int additionalMinutes);

        /// <summary>
        /// Gets the total reserved quantity for a specific product.
        /// </summary>
        Task<int> GetTotalReservedQuantityForProductAsync(Guid productId);
    }

}
