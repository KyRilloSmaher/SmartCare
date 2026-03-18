using MediatR;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Store.Queries.GetStorePharmcists
{
    public record GetStorePharmcistsQuery(Guid storeId) : IRequest<Response<IEnumerable<PharmacistResponseDto>>>;
}
