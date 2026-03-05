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

        public static bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
        {
            // Example rules — adapt to your domain
            if (from == to) return false;
            if (from == OrderStatus.Cancelled || from == OrderStatus.Completed || from == OrderStatus.Expired) return false;

            // Allow any transition for this template except the disallowed above
            return true;
        }
       
    }
}
