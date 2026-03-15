using MediatR;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Commands.ChangeCategoryLogo
{
    public record ChangeCategoryLogoCommand(ChangeCategoryLogoRequestDto CategoryDto) : IRequest<Response<string>>;
}
