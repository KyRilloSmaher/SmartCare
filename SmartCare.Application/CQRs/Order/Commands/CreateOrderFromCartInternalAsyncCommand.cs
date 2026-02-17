using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Commands
{
    public record CreateOrderFromCartInternalAsyncCommand<T>(string clientId, Guid cartId, OrderType orderType, Guid? storeId, Guid? deliveryAddressId) : IRequest<Response<T?>>;
}
