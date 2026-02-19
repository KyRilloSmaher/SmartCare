using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Commands
{
    public record RebuildOrderFromCartAsyncCommand(SmartCare.Domain.Entities.Order order, SmartCare.Domain.Entities.Cart cart, IEnumerable<CartItem> cartItems, OrderType newOrderType, Guid? shippingAddressId, Guid? storeId) : IRequest<Response<OrderResponseDto>>;
}
