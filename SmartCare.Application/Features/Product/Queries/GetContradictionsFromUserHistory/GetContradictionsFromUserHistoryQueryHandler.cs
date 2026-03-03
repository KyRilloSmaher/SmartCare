using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.Features.Product.Queries.RecommendSimilarProducts;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.GetContradictionsFromUserHistory
{
    public class GetContradictionsFromUserHistoryQueryHandler : IRequestHandler<GetContradictionsFromUserHistoryQuery, Response<ICollection<ProductResponseDtoForClient>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<GetContradictionsFromUserHistoryQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiServices _aiServices;

        public GetContradictionsFromUserHistoryQueryHandler(IResponseHandler responseHandler, IMediator mediator, ILogger<GetContradictionsFromUserHistoryQueryHandler> logger, IUnitOfWork unitOfWork, IMapper mapper, IAiServices aiServices)
        {
            _responseHandler = responseHandler;
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiServices = aiServices;
        }

        public async Task<Response<ICollection<ProductResponseDtoForClient>>> Handle(GetContradictionsFromUserHistoryQuery request,CancellationToken cancellationToken)
        {
            // 1. Fetch purchase history
            var userHistory = await _unitOfWork.Clients
                .GetClientPurchasesHistoryAsync(request.UserId);

            if (userHistory is null || !userHistory.Any())
            {
                _logger.LogInformation(
                    "No purchase history found for client {UserId}", request.UserId);

                return _responseHandler.Success<ICollection<ProductResponseDtoForClient>>(
                    [],
                    "No previous purchases found for this client.");
            }

            // 2. Ask AI which history IDs contradict the requested product
            var contradictingIds = await _aiServices.CheckContradictionsAsync(request.ProductId, userHistory.ToList());

            if (contradictingIds is null || !contradictingIds.Contradictions.Any())
            {
                _logger.LogInformation(
                    "No contradictions found for product {ProductId} against {Count} history item(s)",
                    request.ProductId, userHistory.Count);

                return _responseHandler.Success<ICollection<ProductResponseDtoForClient>>(
                    [],
                    "No contradictions found with your previous purchases.");
            }

            // 3. Hydrate products in parallel — avoids N serial DB round-trips
            var products = await Task.WhenAll(
                contradictingIds.Contradictions.Select(c => _unitOfWork.Products.GetByIdAsync(Guid.Parse(c.Id))));

            // 4. Filter nulls (AI may reference stale IDs no longer in DB) then map
            ICollection<ProductResponseDtoForClient> result = products
                .Where(p => p is not null)
                .Select(p => _mapper.Map<ProductResponseDtoForClient>(p!))
                .ToList();

            _logger.LogInformation(
                "Product {ProductId} contradicts {Count} item(s) in client {UserId} history",
                request.ProductId, result.Count, request.UserId);

            return _responseHandler.Success(
                result,
                "Some of your previous purchases have a medical contradiction with this product.");
        }
    }
}
