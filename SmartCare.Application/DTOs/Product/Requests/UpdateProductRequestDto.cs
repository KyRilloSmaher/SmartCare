using Microsoft.AspNetCore.Http;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Product.Requests
{
    public class UpdateProductRequestDto
    {
        public string NameEn { get; set; }
        public string? NameAr { get; set; }
        public Guid CategoryId { get; set; }
        public Guid CompanyId { get; set; }
        public string Description { get; set; }
        public string MedicalDescription { get; set; }
        public string Tags { get; set; }
        public float DiscountPercentage { get; set; }
        public string ActiveIngredients { get; set; }
        public string? SideEffects { get; set; }
        public string? Contraindications { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? DosageForm { get; set; }
        public IFormFile mainImage { get; set; }
        public IList<IFormFile> Images { get; set; }


    }
}
