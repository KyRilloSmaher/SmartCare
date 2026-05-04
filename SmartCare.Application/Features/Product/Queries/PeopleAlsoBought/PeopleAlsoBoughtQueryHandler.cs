using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Queries.PeopleAlsoBought
{
    public class PeopleAlsoBoughtQueryHandler: IRequestHandler<PeopleAlsoBoughtQuery, Response<List<ProductResponseDtoForClient>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<PeopleAlsoBoughtQueryHandler> _logger;
        private readonly IDataMiningService _dataMiningService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.PeopleAlsoBounght;


        public PeopleAlsoBoughtQueryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, IMapper mapper, ILogger<PeopleAlsoBoughtQueryHandler> logger, IDataMiningService dataMiningService, IRedisCacheService redisCacheService)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _dataMiningService = dataMiningService;
            _redisCacheService = redisCacheService;
        }

        public async Task<Response<List<ProductResponseDtoForClient>>> Handle(PeopleAlsoBoughtQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check cache first
                var cacheKey = $"PeopleAlsoBought_{request.ProductId}";
                var cachedResult = await _redisCacheService.GetDataAsync<List<ProductResponseDtoForClient>>(cacheKey,tag);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for PeopleAlsoBoughtQuery with ProductId: {ProductId}", request.ProductId);
                    return _responseHandler.Success(cachedResult);
                }
                _logger.LogInformation("Cache miss for PeopleAlsoBoughtQuery with ProductId: {ProductId}", request.ProductId);
                var transactions = await _unitOfWork.Orders.GetTransactionsAsync();
                var freq = await _dataMiningService.GenerateFrequentItemsetsAsync(transactions,0.6);
                var rules =  await _dataMiningService.GenerateAssociationRulesAsync(freq,0.75);
                // Get related product IDs from data mining service
                var relatedProductIds = await _dataMiningService.GetRecommendationsAsync(request.ProductId, rules, 10);
                if (relatedProductIds == null || !relatedProductIds.Any())
                {
                    return _responseHandler.Success(new List<ProductResponseDtoForClient>());
                }
                // Fetch product details from repository
                var products = await _unitOfWork.Products.GetProductsByIds(relatedProductIds);
                var productDtos = _mapper.Map<List<ProductResponseDtoForClient>>(products);
                // Cache the result
                await _redisCacheService.SetDataAsync(tag, productDtos, cacheKey, TimeSpan.FromHours(12));
                return _responseHandler.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling PeopleAlsoBoughtQuery for ProductId: {ProductId}", request.ProductId);
                return _responseHandler.Failed<List<ProductResponseDtoForClient>>("An error occurred while processing your request.");
            }
        }

     }
}
