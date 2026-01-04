using SmartCare.Domain.Enums;
using System;

namespace SmartCare.Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Optional: link to order item for Pickup/Order-based reservations
        /// </summary>
        public Guid? OrderItemId { get; set; }

        /// <summary>
        /// Product associated with this reservation
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Inventory where the reserved stock is taken from
        /// </summary>
        public Guid InventoryId { get; set; }

        /// <summary>
        /// Quantity reserved in inventory
        /// </summary>
        public int QuantityReserved { get; set; }

        /// <summary>
        /// When the reservation was created
        /// </summary>
        public DateTime ReservedAt { get; set; }

        /// <summary>
        /// When the reservation will expire
        /// </summary>
        public DateTime ExpiredAt { get; set; }

        /// <summary>
        /// Current reservation status
        /// </summary>
        public ReservationStatus Status { get; set; }

        /// <summary>
        /// Navigation property for order item (if applicable)
        /// </summary>
        public OrderItem? OrderItem { get; set; }
    }
}


