using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public sealed class CreatePaymentSessionCommand
    {
        public Guid OrderId { get; init; }
        public string ClientId { get; init; }

        public decimal Amount { get; init; }
        public string Currency { get; init; } = "Egp";

        public PaymentMethod Provider { get; init; }

        public string? CallbackUrl { get; init; } = default!;
    }
}
