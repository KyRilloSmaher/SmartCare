using Microsoft.Extensions.Options;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Domain.Helpers;
using Stripe.Checkout;
using Stripe;

public class StripeService : IPaymentGetway
{
    private readonly StripeSettings _stripeSettings;

    public StripeService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        decimal amount,
        string successUrl,
        string cancelUrl,
        string clientReferenceId,
        string currency = "egp",
        Dictionary<string, string>? metadata = null)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = amount * 100, // Stripe uses cents
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "SmartCare Order"
                        }
                    },
                    Quantity = 1
                }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = clientReferenceId,
            Metadata = metadata
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

    public bool VerifyWebhookSignature(string json, string stripeSignatureHeader, string webhookSecret, out Event stripeEvent)
    {
        stripeEvent = null!;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, webhookSecret);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
