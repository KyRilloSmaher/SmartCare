using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Contradictions.Response;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Product.Queries.GetContradictionsFromUserHistory
{
    public class GetContradictionsFromUserHistoryQueryHandler : IRequestHandler<GetContradictionsFromUserHistoryQuery, Response<List<ContradictionDetail>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<GetContradictionsFromUserHistoryQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiServices _aiServices;

        public GetContradictionsFromUserHistoryQueryHandler(
            IResponseHandler responseHandler,
            IMediator mediator,
            ILogger<GetContradictionsFromUserHistoryQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAiServices aiServices)
        {
            _responseHandler = responseHandler;
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiServices = aiServices;
        }

        public async Task<Response<List<ContradictionDetail>>> Handle(
            GetContradictionsFromUserHistoryQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Fetch purchase history with dates
                var userHistory = await _unitOfWork.Clients
                    .GetClientPurchasesHistoryWithDatesAsync(request.UserId);

                if (userHistory is null || !userHistory.Any())
                {
                    _logger.LogInformation(
                        "No purchase history found for client {UserId}", request.UserId);

                    return _responseHandler.Success<List<ContradictionDetail>>(
                        [],
                        "No previous purchases found for this client.");
                }

                // 2. Get the requested product with its ingredients
                var requestedProduct = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
                if (requestedProduct == null)
                {
                    return _responseHandler.NotFound<List<ContradictionDetail>>(
                        "Product not found.");
                }

                // Parse active ingredients from the product
                var requestedIngredients = ParseIngredients(requestedProduct.ActiveIngredients);

                // 3. Check each history product for contradictions
                var contradictionDetails = new List<ContradictionDetail>();
                var processedProductIds = new HashSet<Guid>();

                foreach (var historyItem in userHistory)
                {
                    var historyProduct = await _unitOfWork.Products.GetByIdAsync(historyItem.ProductId);
                    if (historyProduct == null || processedProductIds.Contains(historyProduct.ProductId))
                        continue;

                    var historyIngredients = ParseIngredients(historyProduct.ActiveIngredients);

                    // Check for contradictions between ingredients
                    foreach (var reqIngredient in requestedIngredients)
                    {
                        foreach (var histIngredient in historyIngredients)
                        {
                            var contradiction = await _unitOfWork.Contradictions
                                .ContradictionExistsAsync(reqIngredient, histIngredient);

                            if (contradiction != null)
                            {
                                // Use AutoMapper to create the base DTO from Product
                                var contradictionDetail = _mapper.Map<ContradictionDetail>(historyProduct);

                                // Manually set the contradiction-specific properties
                                contradictionDetail.IngredientA = reqIngredient;
                                contradictionDetail.IngredientB = histIngredient;
                                contradictionDetail.Reason = contradiction.Reason;
                                contradictionDetail.Severity = contradiction.Severity;
                                contradictionDetail.SeverityLevel = MapSeverityToLevel(contradiction.Severity);
                                contradictionDetail.PurchaseDate = historyItem.PurchaseDate;

                                contradictionDetails.Add(contradictionDetail);
                                processedProductIds.Add(historyProduct.ProductId);
                                break; // Found contradiction for this product, move to next
                            }
                        }

                        if (processedProductIds.Contains(historyProduct.ProductId))
                            break;
                    }
                }

       

                if (!contradictionDetails.Any())
                {
                    _logger.LogInformation(
                        "No contradictions found for product {ProductId} against {Count} history item(s)",
                        request.ProductId, userHistory.Count);

                    return _responseHandler.Success<List<ContradictionDetail>>(
                        [],
                        "No contradictions found with your previous purchases.");
                }

                _logger.LogInformation(
                    "Product {ProductId} contradicts {Count} item(s) in client {UserId} history",
                    request.ProductId, contradictionDetails.Count, request.UserId);

                return _responseHandler.Success(
                    contradictionDetails,
                    "Some of your previous purchases have a medical contradiction with this product.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking contradictions for product {ProductId} and user {UserId}",
                    request.ProductId, request.UserId);

                return _responseHandler.Failed<List<ContradictionDetail>>(
                    "An error occurred while checking for contradictions.");
            }
        }

        private List<string> ParseIngredients(string? ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredients))
                return new List<string>();

            return ingredients.Split(new[] { ',', ';', '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();
        }

        private int MapSeverityToLevel(string? severity)
        {
            return severity?.ToLower() switch
            {
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }
    }
}