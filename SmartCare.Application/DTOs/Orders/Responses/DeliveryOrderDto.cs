using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class DeliveryOrderDto
    {
        // Order Info
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Client Info
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }

        // Client Address
        public string DeliveryAddressLine { get; set; }
        public string? DeliveryAddressLabel { get; set; }
        public string? DeliveryAddressAdditionalInfo { get; set; }
        public float ClientLatitude { get; set; }
        public float ClientLongitude { get; set; }

        // Store Info
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StorePhone { get; set; }
        public float StoreLatitude { get; set; }
        public float StoreLongitude { get; set; }

        // Distance & Pricing
        public double DistanceKm { get; set; }
        public decimal MedicinePrice { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalPrice { get; set; }

        // Order Items
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string MedicineName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
