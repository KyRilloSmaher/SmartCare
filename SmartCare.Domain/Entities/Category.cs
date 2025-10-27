using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int ProductsCount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public  ICollection<Product> Products { get; set; }
    }
}
