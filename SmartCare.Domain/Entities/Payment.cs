using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        /// <summary>
        /// Link to the Order
        /// </summary>
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        /// <summary>
        /// Stripe PaymentIntent ID
        /// </summary>
        public string? PaymentIntentId { get; set; } = null!;

        /// <summary>
        /// Client secret for frontend payment confirmation
        /// </summary>
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Amount in order currency
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Version for concurrency / payment update
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Current payment status
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Payment method used
        /// </summary>
        public PaymentMethod Method { get; set; } = PaymentMethod.Stripe;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
