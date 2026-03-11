using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public class PaymentIntentResponse
    {
        /// <summary>
        /// Stripe client secret used by frontend
        /// </summary>
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Stripe PaymentIntent ID
        /// </summary>
        public string PaymentIntentId { get; set; } = null!;

        /// <summary>
        /// Amount to be paid
        /// </summary>
        public decimal Amount { get; set; }
    }

}
