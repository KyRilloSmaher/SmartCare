using AutoMapper;
using MediatR;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class CreatePickupOrderFromCartAsyncHandler : IRequestHandler<CreatePickupOrderFromCartAsyncCommand, Response<PickUpOrderResponseDto?>>
    {
        #region Fields
        private readonly IMediator _mediator;
        #endregion

        public CreatePickupOrderFromCartAsyncHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Response<PickUpOrderResponseDto?>> Handle(CreatePickupOrderFromCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var dto = request.dto;
            return await _mediator.Send(new CreateOrderFromCartInternalAsyncCommand<PickUpOrderResponseDto?>(
                clientId, dto.CartId, OrderType.InStore, dto.storeId, null));
        }
    }
}