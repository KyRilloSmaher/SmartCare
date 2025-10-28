using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class ProductProjectionDTO
    {
        public Guid ProductId { get; set; }
        public string ProductNameAr { get; set; }
        public string ProductNameEn { get; set; }
        public string Description { get; set; }
        public float AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
