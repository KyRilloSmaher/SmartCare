using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Orders.Queries.GetTodayOnlineOrders
{
    public class GetTodayOnlineOrdersHandler
        : IRequestHandler<GetTodayOnlineOrdersQuery, Response<PaginatedResult<OnlineOrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapService _mapService;
        private readonly string tag = CacheConstants.Orders; 
        #endregion

        public GetTodayOnlineOrdersHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapService mapService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapService = mapService;
        }

        public async Task<Response<PaginatedResult<OnlineOrderResponseDto>>> Handle(
            GetTodayOnlineOrdersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;
            var storeId = request.StoreId;

            // 1. Validate pagination
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<OnlineOrderResponseDto>>(
                    SystemMessages.INVALID_PAGINATION_PARAMETERS);

            // 2. Get store
            var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
            if (store == null)
                return _responseHandler.NotFound<PaginatedResult<OnlineOrderResponseDto>>(
                    SystemMessages.STORE_NOT_FOUND);

            // 3. Check cache
            string cacheKey = $"orders_today_online_store_{storeId}_p{pageNumber}_s{pageSize}";
            try
            {
                var cachedData = await _redisCacheService
                    .GetDataAsync<PaginatedResult<OnlineOrderResponseDto>>(cacheKey, tag);
                if (cachedData != null)
                    return _responseHandler.Success(cachedData);
            }
            catch (Exception) { }

            // 4. Fetch from DB
            var orders = await _unitOfWork.Orders
                .GetTodayOnlineOrdersByStore(storeId)
                .ToListAsync(cancellationToken);

            if (!orders.Any())
                return _responseHandler.NotFound<PaginatedResult<OnlineOrderResponseDto>>(
                    SystemMessages.NOT_FOUND);

            // 5. Sort in memory
            var completedStatuses = new[]
            {
                OrderStatus.Completed,
                OrderStatus.Cancelled,
                OrderStatus.Returned,
                OrderStatus.Refunded,
                OrderStatus.Expired
            };

            var sorted = orders
                .Select(o => new
                {
                    Order = o,
                    Distance = _mapService.CalculateDistanceKm(
                                    store.Latitude, store.Longitude,
                                    o.Address.Latitude, o.Address.Longitude),
                    IsCompleted = completedStatuses.Contains(o.Status)
                })
                .OrderBy(o => o.IsCompleted)     
                .ThenBy(o => o.Order.CreatedAt)  
                .ThenBy(o => o.Distance)         
                .ToList();

            // 6. Map to DTO
            var mappedOrders = sorted.Select(o => new OnlineOrderResponseDto
            {
                OrderId = o.Order.Id,
                ClientName = $"{o.Order.Client.User.FirstName} {o.Order.Client.User.LastName}",
                ClientPhone = o.Order.Client.User.PhoneNumber,
                TotalPrice = o.Order.TotalPrice,
                Status = o.Order.Status.ToString(),
                OrderDate = o.Order.CreatedAt,
                DistanceFromBranch = Math.Round(o.Distance, 2),
                DeliveryAddress = o.Order.Address.AddressLine,
                AdditionalInfo = o.Order.Address.AdditionalInfo,
                Items = o.Order.Items.Select(oi => new OnlineOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.NameEn,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubTotal = oi.SubTotal
                }).ToList()
            }).ToList();

            var paginatedResult = PaginatedResult<OnlineOrderResponseDto>.Success(
                items: mappedOrders.Skip((pageNumber - 1) * pageSize).Take(pageSize),
                totalCount: mappedOrders.Count,
                pageNumber: pageNumber,
                pageSize: pageSize);

            // 8. Cache result
            try
            {
                await _redisCacheService.SetDataAsync(cacheKey, paginatedResult, tag, Time.Default);
            }
            catch (Exception) { }

            return _responseHandler.Success(paginatedResult);
        }
    }
}