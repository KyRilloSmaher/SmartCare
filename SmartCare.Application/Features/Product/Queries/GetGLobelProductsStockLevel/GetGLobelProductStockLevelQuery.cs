using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;


namespace SmartCare.Application.Features.Product.Queries.GetGLobelProductsStockLevel
{
    public record GetGLobelProductStockLevelQuery(int pageNumber, int pageSize) : IRequest<Response<PaginatedResult<GLobelProductStockLevel>>>;

}
