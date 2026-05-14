using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class OnlineOrder : Order
    {
        public Guid ShippingAddressId { get; set; }
        [ForeignKey(nameof(ShippingAddressId))]
        public Address Address { get; set; }
        public decimal? DeleiveryFees { get; set; } = 0;
        public string? DeliveryId { get; set; }
        public Delivery Delivery { get; set; }

        public OnlineOrder() { }

        public OnlineOrder(string clientId, decimal totalPrice, Guid deliveryAddressId)
        {
            ClientId = clientId;
            TotalPrice = totalPrice;
            ShippingAddressId = deliveryAddressId;
            OrderType = Enums.OrderType.Online;
            Status = Enums.OrderStatus.Pending;
        }
        public static OnlineOrder Create(string clientId , decimal totalPrice , Guid deliveryAddressId )
        {
            return new OnlineOrder( clientId,  totalPrice,  deliveryAddressId);
        }
        public decimal GetTotalFees() => TotalPrice + (DeleiveryFees ?? 0);
    }
}
