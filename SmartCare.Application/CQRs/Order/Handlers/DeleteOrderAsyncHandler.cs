using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
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
    public class DeleteOrderAsyncHandler : IRequestHandler<DeleteOrderAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        #endregion
        public DeleteOrderAsyncHandler(IResponseHandler responseHandler, IOrderRepository orderRepository, IMediator mediator)
        {
            _responseHandler = responseHandler;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<Response<bool>> Handle(DeleteOrderAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.BAD_REQUEST);

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status == OrderStatus.Pending)
                await _mediator.Send(new ReleaseOrderReservationsAsyncCommand(orderId));

            var deleted = await _orderRepository.DeleteAsync(order);
            if (!deleted)
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);

            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }
    }
}
