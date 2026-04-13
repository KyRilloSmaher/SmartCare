using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.Clients
{
	public record GetClientAnalyticsQuery(Guid? BranchId , DateTime? StartDate , DateTime? EndDate, FilterIntervales interval = FilterIntervales.monthly) : IRequest<Response<ClientAnalyticsDto>>;
}