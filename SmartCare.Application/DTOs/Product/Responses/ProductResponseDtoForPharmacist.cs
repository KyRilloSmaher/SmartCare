using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Product.Responses
{
    public class ProductResponseDtoForPharmacist
    {
        public Guid ProductId { get; set; }
        public string NameEn { get; set; }
        public string? NameAr { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public float DiscountPercentage { get; set; }
        public float AverageRating { get; set; }
        public bool IsAvailable { get; set; }
        public string? DosageForm { get; set; }
        public int StockQuantity { get; set; }
        public int AvailableStock { get; set; }
        public List<string> ImageUrls { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }
}
