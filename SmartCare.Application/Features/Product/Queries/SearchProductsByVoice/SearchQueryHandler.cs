using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

namespace SmartCare.Application.Features.Product.Queries.VoiceSearch
{
    public class SearchQueryHandler : IRequestHandler<VoiceSearchQuery, Response<IEnumerable<ProductResponseDtoForClient>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IMediator _mediator;
        private readonly ILogger<SearchQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiServices _aiServices;
        private readonly IMapper _mapper;
        public SearchQueryHandler(IResponseHandler responseHandler, IMediator mediator, ILogger<SearchQueryHandler> logger, IUnitOfWork unitOfWork, IAiServices aiServices, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aiServices = aiServices;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ProductResponseDtoForClient>>> Handle( VoiceSearchQuery request,CancellationToken cancellationToken)
        {
            // AI voice search
            var seearchResponse = await _aiServices.VoiceSearchAsync(request.audioFile);

            var searchResults = seearchResponse?.Results ?? [];

            if (!searchResults.Any())
            {
                return _responseHandler.Success<IEnumerable<ProductResponseDtoForClient>>(
                    [],
                    "No products found matching your search.");
            }

            
            var searchIds = searchResults
                .Select(r => Guid.Parse(r.Id))
                .ToList();


            var products = await _unitOfWork.Products.FilterListAsync(
                p => searchIds.Contains(p.ProductId)
            );

            if (!products.Any())
            {
                return _responseHandler.Success<IEnumerable<ProductResponseDtoForClient>>(
                    [],
                    "No products found matching your search.");
            }

            //Build score map for ranking
            var scoreMap = searchResults.ToDictionary(
                r => Guid.Parse(r.Id),
                r => r.Score
            );

            //Rank products by AI score
            var rankedProducts = products
                .OrderByDescending(p => scoreMap.TryGetValue(p.ProductId, out var score) ? score : 0)
                .ToList();

            //Map to DTO
            var result = rankedProducts
                .Select(p => _mapper.Map<ProductResponseDtoForClient>(p));

            _logger.LogInformation("voice Search '{fileName}' => {Total} results | voice hits={S}", request.audioFile.FileName, result.Count(),searchResults.Count);

            return _responseHandler.Success(result);
        }
    }
}
