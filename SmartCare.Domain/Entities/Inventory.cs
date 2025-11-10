using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public Guid ProductId { get; set; }
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public Store Store { get; set; }
        public Product Product { get; set; }
        public  OrderItem OrderItem { get; set; }
        public  ICollection<CartItem> CartItems { get; set; }

    }
}
