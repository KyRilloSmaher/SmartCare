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
            Task<Reservation?> CreateReservationAsync(Guid CartItemId ,int quantity, ReservationStatus status);
            Task<bool> CancelReservationAsync(Reservation reservation , ReservationStatus status);
            Task<bool> UpdateReservationQuantityAsync(Reservation reservation, int newQuantity);
            Task<bool> ReleaseAllReservationsForCartAsync(Guid cartId);
            Task<bool> RemoveAllExpiredReservationsAsync();
            Task<IEnumerable<Reservation>> GetActiveReservationsByProductAsync(Guid productId);
            Task<Reservation?> GetReservationByCartItemIdAsync(Guid cartItemId);
        Task FinalizeStockDeductionAsync(Guid cartId, IEnumerable<Guid> orderedProductIds);
        }
  
}
