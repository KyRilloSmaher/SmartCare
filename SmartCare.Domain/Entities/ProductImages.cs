using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public bool IsPrimary { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
