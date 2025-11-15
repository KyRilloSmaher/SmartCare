using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Product.Responses
{
    public class ProductResponseDtoForClient
    {
        public Guid ProductId { get; set; }
        public string MainImageUrl { get; set; }
        public IList<string> Images { get; set; }
        public string NameEn { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public float AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string ActiveIngredients { get; set; }
        public float DiscountPercentage { get; set; }

    }
}
