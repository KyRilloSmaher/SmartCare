
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace SmartCare.Domain.Entities
{
    public class Store
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        [Required]
        public float Latitude { get; set; }

        [Required]
        public float Longitude { get; set; }

        /// <summary>
        /// Optional spatial column (SQL Server)
        /// Useful for fast STDistance queries.
        /// </summary>
        public Point? GeoLocation { get; set; }

        [Required]
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public  ICollection<FromStoreOrder> Orders { get; set; }
        public  ICollection<Inventory> Inventories { get; set; }

    }
}


