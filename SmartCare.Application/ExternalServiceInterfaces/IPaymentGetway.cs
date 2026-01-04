using Stripe.Checkout;
using Stripe;
using SmartCare.Application.DTOs.Payment;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IPaymentGetway
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string orderId, int version);
        Task<PaymentIntent> UpdatePaymentIntentAmountAsync(string intentId, decimal amount);
        Task CancelPaymentIntentAsync(string intentId);
        Task<Session> CreateCheckoutSessionAsync(PaymentSessionRequest request);
        bool VerifyWebhookSignature(string json, string signature, string secret, out Event webhookEvent);
        Task<bool> RefundPaymentAsync(string sessionId);
    }
}
