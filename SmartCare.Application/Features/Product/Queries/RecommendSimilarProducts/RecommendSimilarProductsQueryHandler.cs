using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.RecommendSimilarProducts
{
    public class RecommendSimilarProductsQueryHandler : IRequestHandler<RecommendSimilarProductsQuery, Response<ICollection<ProductResponseDtoForClient>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IMediator _mediator;
        private readonly ILogger<RecommendSimilarProductsQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAiServices _aiServices;
        public RecommendSimilarProductsQueryHandler(IResponseHandler responseHandler, IMediator mediator, ILogger<RecommendSimilarProductsQueryHandler> logger, IUnitOfWork unitOfWork, IMapper mapper, IAiServices aiServices)
        {
            _responseHandler = responseHandler;
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiServices = aiServices;
        }

        public async Task<Response<ICollection<ProductResponseDtoForClient>>> Handle(RecommendSimilarProductsQuery request,CancellationToken cancellationToken)
        {
            // 1. Ask AI for similar product IDs
            var similarIds = await _aiServices.GetSimilarProductsAsync(request.ProductId);

            if (similarIds is null || !similarIds.Results.Any())
            {
                _logger.LogInformation(
                    "AI returned no similar products for {ProductId}", request.ProductId);

                return _responseHandler.Success<ICollection<ProductResponseDtoForClient>>(
                    [],
                    "No similar products found.");
            }

            // 2. Hydrate in parallel
            var products = await Task.WhenAll(
                similarIds.Results.Select(r => _unitOfWork.Products.GetByIdAsync(Guid.Parse(r.Id))));

            // 3. Filter nulls (stale AI IDs) then map
            ICollection<ProductResponseDtoForClient> result = products
                .Where(p => p is not null)
                .Select(p => _mapper.Map<ProductResponseDtoForClient>(p!))
                .ToList();

            if (!result.Any())
            {
                _logger.LogWarning(
                    "AI returned {Count} ID(s) for {ProductId} but none resolved in DB",
                    similarIds.Results.Count(), request.ProductId);

                return _responseHandler.Success<ICollection<ProductResponseDtoForClient>>(
                    [],
                    "No similar products could be resolved.");
            }

            _logger.LogInformation(
                "Returning {Count} similar product(s) for {ProductId}",
                result.Count, request.ProductId);

            return _responseHandler.Success(result);
        }
    }
}
