using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
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
        private readonly IMapService _mapService;
        #endregion

        public GetTodayOnlineOrdersHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapService mapService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapService = mapService;
        }

        public async Task<Response<PaginatedResult<OnlineOrderResponseDto>>> Handle(
            GetTodayOnlineOrdersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;
            var storeId = request.StoreId;

            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<OnlineOrderResponseDto>>(
                    SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
            if (store == null)
                return _responseHandler.NotFound<PaginatedResult<OnlineOrderResponseDto>>(
                    SystemMessages.STORE_NOT_FOUND);

            var orders = await _unitOfWork.Orders
                .GetTodayOnlineOrdersByStore(storeId)
                .ToListAsync(cancellationToken);

            if (orders == null || !orders.Any())
                return _responseHandler.Success(
                    new PaginatedResult<OnlineOrderResponseDto>
                    {
                        Items = new List<OnlineOrderResponseDto>(),
                        TotalCount = 0,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    });

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

            // 5. Map to DTO
            var mappedOrders = sorted.Select(o => new OnlineOrderResponseDto
            {
                OrderId = o.Order.Id,
                ClientName = $"{o.Order.Client.User.FirstName} {o.Order.Client.User.LastName}",
                ClientPhone = o.Order.Client.User.PhoneNumber,
                TotalPrice = o.Order.TotalPrice,
                Status = o.Order.Status.ToString(),
                OrderDate = o.Order.CreatedAt,
                Is_paid = o.Order.Payment != null,
                DeliveryFees = o.Order.DeleiveryFees,
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

            return _responseHandler.Success(paginatedResult);
        }
    }
}