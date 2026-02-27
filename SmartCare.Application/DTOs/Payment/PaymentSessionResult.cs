using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public class PaymentSessionResult
    {
        public string ProviderReferenceId { get; init; } = default!;
        public string? ClientPaymentToken { get; init; }
        public PaymentMethod Provider { get; init; }
    }
}
