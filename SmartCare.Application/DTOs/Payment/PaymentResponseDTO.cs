using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Payment
{
    public class PaymentResponseDTO
    {
        public Guid Id { get; set; }
        public Guid OrderId { get;  set; }
        public string ProviderReferenceId { get;  set; } = default!;
        public decimal Amount { get;  set; }
        public PaymentStatus Status { get;  set; }
        public PaymentMethod Method { get;  set; }
        public DateTime CreatedAt { get;  set; }
        public DateTime? UpdatedAt { get;  set; }
        public DateTime? CompletedAt { get;  set; }
    }
}
