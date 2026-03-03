using MediatR;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Commands
{
    public record CreateProductAsyncCommand(CreateProductRequestDto ProductDto) : IRequest<Response<ProductResponseDtoForAdmin>>;
}
