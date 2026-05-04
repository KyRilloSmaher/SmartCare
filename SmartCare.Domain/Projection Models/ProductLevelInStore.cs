using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class ProductLevelInStore
    {
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
