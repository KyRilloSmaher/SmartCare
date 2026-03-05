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
        public int AvailableStock => StockQuantity - ReservedQuantity;
        public Store Store { get; set; }
        public Product Product { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }


        public bool Reserve(int qty)
        {
            if (AvailableStock < qty)
                return false;

            ReservedQuantity += qty;
            return true;
        }

        public void Release(int qty)
        {
           ReservedQuantity = Math.Max(0, ReservedQuantity - qty);
        }
        public void Confirm(int qty)
        {
            ReservedQuantity -= qty;
            StockQuantity -= qty;
        }
    }
}
