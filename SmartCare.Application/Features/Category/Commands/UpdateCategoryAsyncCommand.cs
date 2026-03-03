using MediatR;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Commands
{
    public record UpdateCategoryAsyncCommand(Guid Id, UpdateCategoryRequest CategoryDto) : IRequest<Response<CategoryResponseDto>>;
}
