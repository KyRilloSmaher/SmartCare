using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.Application.Features.Store.Commands.Restore
{
    public record RestoreStoreCommand(Guid Id) : IRequest<Response<bool>>;
}
