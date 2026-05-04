using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Features.Orders.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Handlers
{
    public class GetTodayPickUpOrdersQueryHandler
       : IRequestHandler<GetTodayPickUpOrdersQuery, Response<PaginatedResult<PickUpOrderNotificationDto>>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;

        public GetTodayPickUpOrdersQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        private static int GetStatusPriority(OrderStatus status) => status switch
        {
            OrderStatus.Pending => 1,
            OrderStatus.Confirmed => 2,
            OrderStatus.Processing => 3,
            OrderStatus.WaitingForPickup => 4,
            OrderStatus.Completed => 5, 
            _ => 6
        };

        public async Task<Response<PaginatedResult<PickUpOrderNotificationDto>>> Handle(
            GetTodayPickUpOrdersQuery request,
            CancellationToken cancellationToken)
        {

            var today = DateTime.UtcNow.Date;

            var orders = await _unitOfWork.Orders
                .GetTodayPickUpOrdersByStoreAsync(request.StoreId, today);

            if (orders == null || !orders.Any())
                return _responseHandler.Success(
                    new PaginatedResult<PickUpOrderNotificationDto>
                    {
                        Items = new List<PickUpOrderNotificationDto>(),
                        TotalCount = 0,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    });


            var sorted = orders
                .OrderBy(o => GetStatusPriority(o.Status)) 
                .ThenBy(o => o.CreatedAt)                  
                .ToList();

            var totalCount = sorted.Count;
            var paged = sorted
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = paged.Select(o => new PickUpOrderNotificationDto
            {
                OrderId = o.Id,
                ClientName = $"{o.Client?.User?.FirstName} {o.Client?.User?.LastName}",
                ClientPhone = o.Client?.User?.PhoneNumber,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString(),
                OrderDate = o.CreatedAt,
                Is_paid = o.Payment != null,
                PickupCode = o.PickupCodeHash,
                Items = o.Items?.Select(oi => new PickUpOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.NameEn,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubTotal = oi.SubTotal
                }).ToList() ?? new List<PickUpOrderItemDto>()
            }).ToList();


            return _responseHandler.Success(
                new PaginatedResult<PickUpOrderNotificationDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
        }
    }
}
