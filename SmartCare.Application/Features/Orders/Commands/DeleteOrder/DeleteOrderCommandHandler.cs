using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        #endregion

        public DeleteOrderCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Response<bool>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status == OrderStatus.Pending)
                await RealseOrder(order.Id);

            await _unitOfWork.Orders.DeleteAsync(order);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);


            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        public async Task  RealseOrder(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order is null)
                return;

            if (order.Items is null || !order.Items.Any())
                return;
            var reservationStatus = ReservationStatus.PaymentTimeOut;
            // Realse All Items Reservations
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

            order.Status = OrderStatus.Cancelled;

        }
    }
}