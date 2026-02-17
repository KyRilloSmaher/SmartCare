using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Commands;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.DTOs.Orders.Responses;
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
    public class UpdateOrderAsyncHandler : IRequestHandler<UpdateOrderAsyncCommand, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        #endregion

        public UpdateOrderAsyncHandler(IResponseHandler responseHandler, ICartRepository cartRepository, IClientRepository clientRepository, IOrderRepository orderRepository, IMediator mediator)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _clientRepository = clientRepository;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<Response<OrderResponseDto>> Handle(UpdateOrderAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var dto = request.dto;
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.USER_NOT_FOUND);

            var cart = await _cartRepository.GetByIdAsync(dto.CartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_EMPTY);

            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(dto.OrderId);
            if (order == null || order.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.ORDER_NOT_EDITABLE);

            return await _mediator.Send(new RebuildOrderFromCartAsyncCommand(
                order,
                cart,
                cartItems,
                dto.UpdatedOrderType,
                dto.ShippingAddressId,
                dto.StoreId));
        }
    }
}
