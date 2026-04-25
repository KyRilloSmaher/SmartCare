using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Categories
{
    public record CategoryChannelsQuery(
        Guid CategoryId,
        DateTime? From = null,
        DateTime? To = null,
        Guid? branchId = null) : IRequest<Response<CategoryChannelDto>>;
}
