using SmartCare.Application.DTOs.payment;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IPaymentService
    {
        Task<Response<Session>> ProcessPaymentAsync(CreateCheckoutSessionRequest req);
        Task<Response<PaymentResult>> MarkPaymentSuccessAsync(Guid orderId);
        Task<Response<PaymentResult>> MarkPaymentFailureAsync(Guid orderId);
        Task<Response<PaymentResult>> TryCancelOrRefundAsync(Guid orderId);
        Task HandleWebhookEventAsync(Event webhookEvent);
    }
}
