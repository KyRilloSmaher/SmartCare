using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Extensions
{
    public static class CartExtensions
    {

        public static async Task<SmartCare.Domain.Entities.Cart?> EnsureCartExistsAsync(
                    this ICartRepository repository, Guid cartId, bool track = false)
                    => await repository.GetByIdAsync(cartId, track);

        public static async Task<SmartCare.Domain.Entities.Product?> EnsureProductExistsAsync(
            this IProductRepository repository, Guid productId)
            => await repository.GetByIdAsync(productId);

        public static async Task<Reservation?> EnsureReservationExistsAsync(
            this IReservationRepository repository, Guid reservationId, bool track = false)
            => await repository.GetByIdAsync(reservationId, track);
    }
}
