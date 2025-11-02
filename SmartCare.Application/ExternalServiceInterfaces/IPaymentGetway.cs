using Stripe.Checkout;
using Stripe;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IPaymentGetway
    {

        Task<Session> CreateCheckoutSessionAsync(decimal amount, string successUrl, string cancelUrl, string clientReferenceId,string currency = "egp" ,Dictionary<string, string>? metadata = null);
        bool VerifyWebhookSignature(string json, string stripeSignatureHeader, string webhookSecret, out Event stripeEvent);
    }
}
