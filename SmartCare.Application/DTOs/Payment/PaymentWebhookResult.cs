using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public sealed class PaymentWebhookResult
    {
        public bool IsValid { get; init; }

        public string ProviderReferenceId { get; init; } = default!;
        public PaymentMethod Provider { get; init; }

        public PaymentStatus Status { get; init; }

        public decimal? Amount { get; init; }
        public Guid? OrderId { get; init; }
        public string? ClientId { get; init; }
    }
}
