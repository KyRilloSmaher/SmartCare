using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Commands.RestoreCategory
{
    public record RestoreCategoryCommand(Guid Id) : IRequest<Response<bool>>;
}
