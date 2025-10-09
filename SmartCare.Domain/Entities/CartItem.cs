using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class CartItem
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ReservationId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public  Cart Cart { get; set; }
        public  Product Product { get; set; }
        public  Inventory Inventory { get; set; }
        public  Reservation Reservation { get; set; }
    }
}
