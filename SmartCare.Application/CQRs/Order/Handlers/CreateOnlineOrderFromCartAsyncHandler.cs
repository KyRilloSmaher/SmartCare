using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.DTOs.Orders.Responses;
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
    public class CreateOnlineOrderFromCartAsyncHandler : IRequestHandler<CreateOnlineOrderFromCartAsyncCommand , Response<OrderResponseDto?>>
    {
        #region Fields
        private readonly IMediator _mediator;
        #endregion

        public CreateOnlineOrderFromCartAsyncHandler(IMediator mediator)
        {
            _mediator = mediator;
        }




        public async Task<Response<OrderResponseDto?>> Handle(CreateOnlineOrderFromCartAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var dto = request.dto;
            return await _mediator.Send(new CreateOrderFromCartInternalAsyncCommand<OrderResponseDto?>(
             clientId, dto.CartId, OrderType.Online, null, dto.deliveryAddressId));
        }
    }
}
