using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Analytics.Categories;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Application.Features.Analytics.Categories
{
    public class CategoryChannelsQueryHandler : IRequestHandler<CategoryChannelsQuery, Response<CategoryChannelDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<CategoryChannelsQueryHandler> _logger;

        public CategoryChannelsQueryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, ILogger<CategoryChannelsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<CategoryChannelDto>> Handle(CategoryChannelsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var channales = await _unitOfWork.Sales.GetCategoryChannelsAsync(request.CategoryId,request.branchId ,request.From, request.To);

                return _responseHandler.Success(channales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category Channels for category {CategoryId}", request.CategoryId);
                return _responseHandler.Failed<CategoryChannelDto>("Failed to retrieve category Channels.");
            }
        }
    }
}
