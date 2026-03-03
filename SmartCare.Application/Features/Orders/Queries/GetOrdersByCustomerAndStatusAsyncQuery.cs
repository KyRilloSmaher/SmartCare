using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Queries
{
    public record GetOrdersByCustomerAndStatusAsyncQuery(string customerId, OrderStatus status) : IRequest<Response<IEnumerable<OrderResponseDto>>>;
}
