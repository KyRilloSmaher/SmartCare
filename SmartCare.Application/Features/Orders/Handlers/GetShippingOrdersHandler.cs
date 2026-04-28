using MediatR;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Orders.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Handlers
{
    public class GetShippingOrdersHandler
    : IRequestHandler<GetShippingOrdersQuery, Response<IEnumerable<DeliveryOrderDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapService _mapService; // ← inject بدل الـ static helper

        public GetShippingOrdersHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            IMapService mapService)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _mapService = mapService;
        }

        public async Task<Response<IEnumerable<DeliveryOrderDto>>> Handle(
            GetShippingOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.Orders.GetShippingOrdersAsync();

            if (!orders.Any())
                return _responseHandler.NotFound<IEnumerable<DeliveryOrderDto>>("No shipping orders found.");

            var result = orders.Select(o =>
            {
                var firstOrderItem = o.Items.FirstOrDefault();

                var store = firstOrderItem?.Inventory?.Store;

                var clientAddress = o is OnlineOrder onlineOrder
                    ? onlineOrder.Address
                    : o.Client?.Addresses?.FirstOrDefault(a => a.IsPrimary && !a.IsDeleted)
                      ?? o.Client?.Addresses?.FirstOrDefault(a => !a.IsDeleted);

                var distanceKm = (store is not null && clientAddress is not null)
                    ? _mapService.CalculateDistanceKm(
                        store.Latitude, store.Longitude,
                        clientAddress.Latitude, clientAddress.Longitude)
                    : 0;

                var medicinePrice = o.Items.Sum(i => i.Quantity * i.UnitPrice);
                var deliveryFee = _mapService.GetDeliveryFee(distanceKm);
                var totalPrice = medicinePrice + deliveryFee;

                return new DeliveryOrderDto
                {
                    OrderId = o.Id,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    ClientName = o.Client != null && o.Client.User != null
                        ? $"{o.Client.User.FirstName} {o.Client.User.LastName}"
                        : "N/A",
                    ClientPhone = o.Client?.User?.PhoneNumber ?? "N/A",
                    DeliveryAddressLine = clientAddress?.AddressLine ?? "N/A",
                    DeliveryAddressLabel = clientAddress?.Label,
                    DeliveryAddressAdditionalInfo = clientAddress?.AdditionalInfo,
                    ClientLatitude = clientAddress?.Latitude ?? 0,
                    ClientLongitude = clientAddress?.Longitude ?? 0,
                    StoreName = store?.Name ?? "N/A",
                    StoreAddress = store?.Address ?? "N/A",
                    StorePhone = store?.Phone ?? "N/A",
                    StoreLatitude = store?.Latitude ?? 0,
                    StoreLongitude = store?.Longitude ?? 0,
                    DistanceKm = Math.Round(distanceKm, 2),
                    MedicinePrice = medicinePrice,
                    DeliveryFee = Math.Round(deliveryFee, 2),
                    TotalPrice = Math.Round(totalPrice, 2),
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        MedicineName = i.Product?.NameEn ?? i.Product?.NameAr ?? "Unknown", 
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        SubTotal = Math.Round(i.Quantity * i.UnitPrice, 2)
                    }).ToList()
                };
            }).ToList();

            return _responseHandler.Success<IEnumerable<DeliveryOrderDto>>(result);
        }
    }
}
