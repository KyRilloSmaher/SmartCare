using Stripe.Checkout;
using Stripe;
using SmartCare.Application.DTOs.Payment;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IPaymentGetway
    {

        Task<Session> CreateCheckoutSessionAsync(PaymentSessionRequest request);
        bool VerifyWebhookSignature(string json, string signature, string secret, out Event webhookEvent);
    }
}
