using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Queries
{
    public record GetOrdersByCustomerIdAsyncQuery(string clientId) : IRequest<Response<IEnumerable<OrderResponseDto>>>;
}
