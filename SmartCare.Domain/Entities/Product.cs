using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{

    public class Product
    {
        public Guid ProductId { get; set; }
        public string? NameAr { get; set; }

        public string NameEn { get; set; }
        public Guid CategoryId { get; set; }

        public Guid CompanyId { get; set; }

        public string Description { get; set; }

        public string MedicalDescription { get; set; }

        public string Tags { get; set; }
        public float AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public float DiscountPercentage { get; set; }
        public string ActiveIngredients { get; set; }
        public string? SideEffects { get; set; }

        public string? Contraindications { get; set; }

        public decimal Price { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsAvailable { get; set; }

        public string? DosageForm { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public Category Category { get; set; }
        public Company Company { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<Rate> Rates { get; set; }
    }
}


