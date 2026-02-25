using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class DeleteOrderAsyncHandler : IRequestHandler<DeleteOrderAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        #endregion

        public DeleteOrderAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Response<bool>> Handle(DeleteOrderAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status == OrderStatus.Pending)
                await _mediator.Send(new ReleaseOrderReservationsAsyncCommand(orderId));

            await _unitOfWork.Orders.DeleteAsync(order);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);


            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }
    }
}