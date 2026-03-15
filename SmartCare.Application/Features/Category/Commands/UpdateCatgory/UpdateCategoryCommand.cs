using MediatR;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Commands
{
    public record UpdateCategoryCommand(UpdateCategoryRequest CategoryDto) : IRequest<Response<CategoryResponseDto>>;
}
