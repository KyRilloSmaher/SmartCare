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

namespace SmartCare.Application.Features.Product.Queries.SearchInProducts
{
    public class SearchQueryHandler : IRequestHandler<SearchQuery, Response<ICollection<ProductResponseDtoForClient>>>
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

        public async Task<Response<ICollection<ProductResponseDtoForClient>>> Handle( SearchQuery request,CancellationToken cancellationToken)
        {
            // 1. Fire AI semantic search and DB text search in parallel
            var semanticTask = _aiServices.SemanticSearchAsync(request.query);

            var dbTask = _unitOfWork.Products.FilterListAsync(
                        p => EF.Functions.Like(p.NameEn, $"%{request.query}%") ||
                             EF.Functions.Like(p.Description, $"%{request.query}%")
                    );
            await Task.WhenAll(semanticTask, dbTask);

            var semanticResults = (await semanticTask)?.Results ?? [];
            var dbProducts = await dbTask;

            var semanticIdSet = semanticResults
                .Select(r => r.Id)
                .ToHashSet();

            var candidates = semanticIdSet.Any()
                ? dbProducts.Where(p => semanticIdSet.Contains(p.ProductId.ToString())).ToList()
                : dbProducts.ToList();

            if (!candidates.Any())
            {
                return _responseHandler.Success<ICollection<ProductResponseDtoForClient>>(
                    [],
                    "No products found matching your search.");
            }

            ICollection<ProductResponseDtoForClient> result;

            if (semanticResults.Any())
            {
                var rankMap = semanticResults
                    .Select((r, index) => new { r.Id, index })
                    .ToDictionary(x => x.Id, x => x.index);

                result = candidates
                    .OrderBy(p => rankMap.TryGetValue(p.ProductId.ToString(), out var rank)
                                    ? rank
                                    : int.MaxValue)
                    .Select(p => _mapper.Map<ProductResponseDtoForClient>(p))
                    .ToList();
            }
            else
            {
                result = candidates
                    .Select(p => _mapper.Map<ProductResponseDtoForClient>(p))
                    .ToList();
            }

            _logger.LogInformation(
                "Search '{Query}' => {Total} result(s) | semantic hits={S} db candidates={D}",
                request.query, result.Count, semanticResults.Count, dbProducts.Count());

            return _responseHandler.Success(result);
        }
    }
}
