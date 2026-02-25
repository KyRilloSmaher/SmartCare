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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Handlers
{
    public class UpdateOrderAsyncHandler : IRequestHandler<UpdateOrderAsyncCommand, Response<OrderResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        #endregion

        public UpdateOrderAsyncHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Response<OrderResponseDto>> Handle(UpdateOrderAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var dto = request.dto;

            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.USER_NOT_FOUND);

            var cart = await _unitOfWork.Carts.GetByIdAsync(dto.CartId, true);
            if (cart == null || cart.ClientId != clientId)
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_NOT_FOUND);

            var cartItems = await _unitOfWork.Carts.GetCartItemsAsync(cart.Id);
            if (!cartItems.Any())
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.CART_EMPTY);

            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(dto.OrderId);
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