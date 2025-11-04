using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string? ClientId { get; set; }
        public Guid? StoreId { get; set; }
        public int? PaymentId { get; set; }
        public AddressResponseDto? Address { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
