using MediatR;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.Features.DashBoard.Queries.GetLowStockProducts;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

public class GetLowStockProductsQueryHandler:IRequestHandler<GetLowStockProductsQuery, Response<PaginatedResult<LowStockProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IResponseHandler _responseHandler;

    public GetLowStockProductsQueryHandler(IUnitOfWork unitOfWork,IResponseHandler responseHandler)
    {
        _unitOfWork = unitOfWork;
        _responseHandler = responseHandler;
    }

    public async Task<Response<PaginatedResult<LowStockProductDto>>> Handle(GetLowStockProductsQuery request,CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        if (pageNumber <= 0 || pageSize <= 0)
            return _responseHandler
                .BadRequest<PaginatedResult<LowStockProductDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
        if (request.Threshold < 0)
        {
            _responseHandler.BadRequest(SystemMessages.INVALID_INPUT);
        }
        var data = _unitOfWork.Inventories.GetLowStockProductsAsync(request.StoreId, request.Threshold);
        var responseData = await data.ToPaginatedListAsync(pageNumber, pageSize); 
        return _responseHandler.Success(responseData);
    }
}