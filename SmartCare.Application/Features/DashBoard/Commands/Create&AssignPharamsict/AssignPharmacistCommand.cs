using MediatR;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.Create_AssignPharamsict
{
    public record AssignPharmacistCommand(Guid StoreId , AssignPharmacistRequest pharmacistRequest) :IRequest<Response<PharmacistResponseDto>>;
}
