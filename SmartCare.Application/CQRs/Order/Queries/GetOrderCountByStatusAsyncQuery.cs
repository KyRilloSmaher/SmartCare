using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Queries
{
    public record GetOrderCountByStatusAsyncQuery(Guid? storeId = null) : IRequest<Response<Dictionary<OrderStatus, int>>>;
}
