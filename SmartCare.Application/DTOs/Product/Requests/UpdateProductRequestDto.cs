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
        public Guid ProductId { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        public string? MedicalDescription { get; set; }
        public string? Tags { get; set; }
        public float? DiscountPercentage { get; set; }
        public string? ActiveIngredients { get; set; }
        public string? SideEffects { get; set; }
        public string? Contraindications { get; set; }
        public decimal? Price { get; set; }
        public bool? IsAvailable { get; set; }
        public string? DosageForm { get; set; }

        //Image Handling
        public IFormFile? NewMainImage { get; set; }
        public IList<IFormFile>? NewImages { get; set; }

        //Remove specific images
        public List<Guid>? RemoveImageIds { get; set; }
    }
}
