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

        public OrderStatus Status { get; protected set; } = OrderStatus.Pending;

        public Guid PaymenId { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Payment? Payment { get; set; }
        public Client Client { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Changes order status using state pattern validation.
        /// </summary>
        public void ChangeStatus(OrderStatus newStatus)
        {
            var state = OrderStates.OrderStateFactory.Create(this);
            state.Handle(this, newStatus);

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Internal method used only by state classes.
        /// </summary>
        internal void SetStatus(OrderStatus status)
        {
            Status = status;
        }
    }
}