using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public Guid StoreId { get; set; }
        public int PaymentId { get; set; }
        public Guid AddressId { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Payment Payment { get; set; }
        public  Client Client { get; set; }
        public  Store Store { get; set; }
        public Address Address { get; set; }
        public  ICollection<OrderItem> Items { get; set; }
    }
}
