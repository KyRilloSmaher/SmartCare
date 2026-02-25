using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class UpdateOrderStatusAsyncHandler : IRequestHandler<UpdateOrderStatusAsyncCommand, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        #endregion

        public UpdateOrderStatusAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Response<OrderResponseDto>> Handle(UpdateOrderStatusAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderId = request.orderId;
            var newStatus = request.newStatus;

            if (orderId == Guid.Empty)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.INVALID_ORDER_STATUS);

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, true);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);
            }

            // Domain rules: prevent illegal transitions
            if (!OrderExtensions.IsValidStatusTransition(order.Status, newStatus))
            {
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);
            }

            order.Status = newStatus;

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Post-update actions (release reservations if cancelled)
            if (newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Expired)
            {
                await _mediator.Send(new ReleaseOrderReservationsAsyncCommand(orderId));
            }

            var dto = _mapper.Map<OrderResponseDto>(order);

            return _responseHandler.Success(dto);
        }
    }
}