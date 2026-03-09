using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System.Collections.Immutable;

namespace SmartCare.Application.Features.Product.Queries.RecommendSimilarProducts
{
    public class RecommendSimilarProductsQueryHandler: IRequestHandler<RecommendSimilarProductsQuery, Response<List<ProductResponseDtoForClient>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<RecommendSimilarProductsQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAiServices _aiServices;

        public RecommendSimilarProductsQueryHandler(IResponseHandler responseHandler,ILogger<RecommendSimilarProductsQueryHandler> logger,IUnitOfWork unitOfWork,IMapper mapper,  IAiServices aiServices)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiServices = aiServices;
        }

        public async Task<Response<List<ProductResponseDtoForClient>>> Handle(RecommendSimilarProductsQuery request,CancellationToken cancellationToken)
        {
            // Ask AI for similar products
            var similarIds = await _aiServices.GetSimilarProductsAsync(request.ProductId);

            if (similarIds == null || !similarIds.Results.Any())
            {
                _logger.LogInformation(
                    "AI returned no similar products for {ProductId}",
                    request.ProductId);

                return _responseHandler.Success<List<ProductResponseDtoForClient>>(
                    [],
                    "No similar products found.");
            }

            //Convert AI IDs to Guid list
            var ids = similarIds.Results
                .Select(r => Guid.Parse(r.Id))
                .ToList();

            // Fetch all products in ONE query
            var products = await _unitOfWork.Products.FilterListAsync(
                p => ids.Contains(p.ProductId));

            if (!products.Any())
            {
                _logger.LogWarning(
                    "AI returned {Count} ID(s) for {ProductId} but none resolved in DB",
                    ids.Count,
                    request.ProductId);

                return _responseHandler.Success<List<ProductResponseDtoForClient>>(
                    [],
                    "No similar products could be resolved.");
            }

            // Preserve AI ranking
            var rankMap = similarIds.Results
                .Select((r, index) => new { r.Id, index })
                .ToDictionary(x => x.Id, x => x.index);

            var orderedProducts = products
                .OrderBy(p => rankMap.TryGetValue(p.ProductId.ToString(), out var rank)
                                ? rank
                                : int.MaxValue).ToImmutableList();

           
            var result = orderedProducts
                .Select(p => _mapper.Map<ProductResponseDtoForClient>(p))
                .ToList();

            _logger.LogInformation(
                "Returning {Count} similar products for {ProductId}",
                result.Count,
                request.ProductId);

            return _responseHandler.Success(result);
        }
    }
}