using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.payment
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? SessionId { get; set; }

        public PaymentResult(bool success, string message, string? sessionId = null)
        {
            Success = success;
            Message = message;
            SessionId = sessionId;
        }
    }
}
