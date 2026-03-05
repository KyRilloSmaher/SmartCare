using MediatR;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Queries.GetCartItems
{
    public record GetCartItemsQuery(Guid cartId) : IRequest<Response<IEnumerable<CartItemResponseDto>>>;
}
