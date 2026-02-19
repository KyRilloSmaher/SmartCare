using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.commens;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Order.Extension
{
    public static class OrderExtensions
    {

        public static SmartCare.Domain.Entities.Order BuildOrder(OrderType orderType, string clientId, decimal totalPrice, Guid? storeId, Guid? deliveryAddressId)
        {
            return orderType switch
            {
                OrderType.InStore => new FromStoreOrder
                {
                    ClientId = clientId,
                    TotalPrice = totalPrice,
                    StoreId = storeId!.Value,
                    OrderType = OrderType.InStore
                },

                OrderType.Online => new OnlineOrder
                {
                    ClientId = clientId,
                    TotalPrice = totalPrice,
                    ShippingAddressId = deliveryAddressId!.Value,
                    OrderType = OrderType.Online
                },

                _ => throw new ArgumentOutOfRangeException(nameof(orderType))
            };
        }

        //public static void ScheduleOrderExpiration(SmartCare.Domain.Entities.Order order)
        //{
        //    var delay = order.OrderType == OrderType.InStore
        //        ? TimeSpan.FromDays(expirationDays)
        //        : TimeSpan.FromHours(expirationHours);

        //    _backgroundJobService.Schedule(
        //        () => ReleaseOrderReservationsAsync(order.Id),
        //        delay);
        //}
        public static List<OrderItem> BuildOrderItems(Guid orderId, IEnumerable<CartItem> cartItems)
        {
            return cartItems.Select(ci => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                SubTotal = ci.SubTotal,
                InvetoryId = ci.InventoryId,
                ReservationId = null // set later after reservation creation
            }).ToList();
        }
        public static OutOfStockItemDto BuildOutOfStock(CartItem ci, int available)
        {
            return new OutOfStockItemDto
            {
                ProductId = ci.ProductId,
                RequestedQty = ci.Quantity,
                AvailableQty = available
            };
        }

        //public static Response<T?> BuildStockErrorResponse<T>(List<OutOfStockItemDto> outOfStock)
        //{
        //    if (typeof(T) == typeof(PickUpOrderResponseDto))
        //    {
        //        return _responseHandler.BadRequest<T?>(
        //            (T)(object)new PickUpOrderResponseDto { outOfStocks = outOfStock },
        //            "Some items are out of stock.");
        //    }

        //    return _responseHandler.Failed<T?>(SystemMessages.INSUFFICIENT_STOCK);
        //}

        public static bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
        {
            // Example rules — adapt to your domain
            if (from == to) return false;
            if (from == OrderStatus.Cancelled || from == OrderStatus.Completed || from == OrderStatus.Expired) return false;

            // Allow any transition for this template except the disallowed above
            return true;
        }
        public static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }
    }
}
