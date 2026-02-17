using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class ReleaseOrderReservationsAsyncHandler : IRequestHandler<ReleaseOrderReservationsAsyncCommand, Unit>
    {
        #region Fields
        private readonly IOrderRepository _orderRepository;
        private readonly IReservationRepository _reservationRepository;

        #endregion
        public ReleaseOrderReservationsAsyncHandler(IOrderRepository orderRepository, IReservationRepository reservationRepository)
        {
            _orderRepository = orderRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<Unit> Handle(ReleaseOrderReservationsAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
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

                await _reservationRepository.CancelReservationAsync(
                    reservationId: item.ReservationId.Value,
                    inventoryId: item.InvetoryId,
                    status: reservationStatus
                );
            }

            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order);

            return Unit.Value;
        }
    }
}
