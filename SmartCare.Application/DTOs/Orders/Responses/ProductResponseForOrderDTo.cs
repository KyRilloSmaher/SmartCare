using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Orders.Responses
{
    public class ProductResponseForOrderDTo
    {
        public Guid ProductId { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public float DiscountPercentage { get; set; }
        public string ImageUrl { get; set; }
    }
}
