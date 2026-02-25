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
        public string? ClientId { get; set; }

        public OrderType OrderType { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        //Payment tracking
        public string? PaymentIntentId { get; set; }
        public int PaymentVersion { get; set; } = 1;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public Payment? Payment { get; set; }
        public Client Client { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

}