using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Analytics.Clients
{
	public record GetClientAnalyticsQuery(Guid? BranchId , string Interval , DateTime? StartDate , DateTime? EndDate) : IRequest<Response<ClientAnalyticsDto>>;
}