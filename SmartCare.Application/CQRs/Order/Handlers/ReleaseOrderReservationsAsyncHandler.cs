using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class ReleaseOrderReservationsAsyncHandler : IRequestHandler<ReleaseOrderReservationsAsyncCommand, Unit>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public ReleaseOrderReservationsAsyncHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ReleaseOrderReservationsAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order == null)
                return Unit.Value;

            // Idempotency: don't re-expire an already finalized order
            if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled or OrderStatus.Completed)
                return Unit.Value;

            if (order.Items == null || !order.Items.Any())
                return Unit.Value;

            var reservationStatus = order.OrderType == OrderType.Online
                ? ReservationStatus.PaymentTimeOut
                : ReservationStatus.PickUpExpired;

            foreach (var item in order.Items)
            {
                if (!item.ReservationId.HasValue)
                    continue;

                await _unitOfWork.Reservations.CancelReservationAsync(
                    reservationId: item.ReservationId.Value,
                    inventoryId: item.InvetoryId,
                    status: reservationStatus
                );
            }

            order.Status = OrderStatus.Expired;

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}