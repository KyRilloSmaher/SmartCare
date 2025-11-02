using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartCare.API.Helpers.ApplicationRouting;

namespace SmartCare.Application.DTOs.Product.Responses
{
    public class ProductResponseDtoForAdmin
    {
        public Guid ProductId { get; set; }
        public ICollection<string> ImageUrls { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public float AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string ActiveIngredients { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CompanyName { get; set; }
        public float DiscountPercentage { get; set; }
        public string? DosageForm { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? SideEffects { get; set; }

        public string? Contraindications { get; set; }
    }
}
