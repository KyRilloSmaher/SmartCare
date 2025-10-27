using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Product.Responses
{
    public class ProductResponseDto
    {
        public Guid ProductId { get; set; }
        public string NameAr { get; set; }

        public string NameEn { get; set; }
        public string Description { get; set; }

        public int TotalRatings { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
