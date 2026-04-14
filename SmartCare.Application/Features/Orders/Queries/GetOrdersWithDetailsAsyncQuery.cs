using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.Application.CQRs.Order.Queries
{
     public record GetOrdersWithDetailsAsyncQuery(int PageNumber , int PageSize) : IRequest<Response<PaginatedResult<OrderResponseDto>>>;
}
