using Microsoft.Extensions.Options;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Domain.Helpers;
using Stripe.Checkout;
using Stripe;
using SmartCare.Application.DTOs.Payment;

public class StripeService : IPaymentGetway
{
    private readonly StripeSettings _stripeSettings;

    public StripeService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<Session> CreateCheckoutSessionAsync(PaymentSessionRequest request)
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
                        UnitAmountDecimal = request.Amount * 100, // Stripe uses cents
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "SmartCare Order"
                        }
                    },
                    Quantity = 1
                }
            },
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            ClientReferenceId = request.OrderId
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

    public bool VerifyWebhookSignature(string json, string signature, string secret, out Event webhookEvent)
    {
        try
        {
            webhookEvent = EventUtility.ConstructEvent(json, signature, secret);
            return true;
        }
        catch (StripeException ex)
        {
            // Optional: log or handle specific Stripe errors
            Console.WriteLine($"⚠️ Stripe webhook verification failed: {ex.Message}");
            webhookEvent = null!;
            return false;
        }
        catch (Exception ex)
        {
            // Optional: catch any other unexpected exception
            Console.WriteLine($"⚠️ Unexpected webhook verification error: {ex.Message}");
            webhookEvent = null!;
            return false;
        }
    }
    /// <summary>
    /// Attempts to refund a payment based on its Stripe Checkout session ID.
    /// </summary>
    public async Task<bool> RefundPaymentAsync(string sessionId)
    {
        try
        {
            // Step 1: Retrieve the checkout session
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);

            if (string.IsNullOrEmpty(session.PaymentIntentId))
            {
                Console.WriteLine($"⚠️ No PaymentIntent found for session {sessionId}");
                return false;
            }

            // Step 2: Create refund
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = session.PaymentIntentId
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);

            // Step 3: Confirm refund success
            return refund.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Stripe refund failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error during refund: {ex.Message}");
            return false;
        }
    }

}
