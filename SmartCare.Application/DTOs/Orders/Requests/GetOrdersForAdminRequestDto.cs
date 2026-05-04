using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class GetOrdersForAdminRequestDto
    {
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
        public string?  ClientId { get; set; } 
        public Guid? BranchId { get; set; }
        public OrderType? OrderType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}
