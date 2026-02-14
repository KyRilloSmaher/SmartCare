using MediatR;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Store.Commands
{
    public record UpdateStoreAsyncCommand(Guid Id, UpdateStoreRequestDto StoreDto) : IRequest<Response<StoreResponseForAdminDto>>;
}
