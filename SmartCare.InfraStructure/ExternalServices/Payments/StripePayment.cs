using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Enums;
using Stripe;
using PaymentMethod = SmartCare.Domain.Enums.PaymentMethod;


namespace SmartCare.InfraStructure.ExternalServices.Payments
{
    public class StripeService : IPaymentGetway
    {
        public SmartCare.Domain.Enums.PaymentMethod Provider => SmartCare.Domain.Enums.PaymentMethod.Stripe;

        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeService> _logger;


        public StripeService(IOptions<StripeSettings> stripeSettings, ILogger<StripeService> logger)
        {
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _logger = logger;
        }

        public async Task<PaymentSessionResult> CreateSessionAsync(CreatePaymentSessionCommand command, CancellationToken cancellationToken = default)
        {
            var service = new PaymentIntentService();

            var intent = await service.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = Convert.ToInt64(command.Amount * 100), // cents
                    Currency = command.Currency.ToLower(),
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        ["OrderId"] = command.OrderId.ToString(),
                        ["ClientId"] = command.ClientId.ToString()
                    }
                },
                cancellationToken: cancellationToken
            );

            return new PaymentSessionResult
            {
                Provider = PaymentMethod.Stripe,
                ProviderReferenceId = intent.Id,
                ClientPaymentToken = intent.ClientSecret
            };
        }

        public async Task CancelSessionAsync(string providerReferenceId, CancellationToken cancellationToken = default)
        {
            var service = new PaymentIntentService();

            await service.CancelAsync(
                providerReferenceId,
                cancellationToken: cancellationToken
            );
        }

        public PaymentWebhookResult ParseWebhook(PaymentMethod provider, string payload, IReadOnlyDictionary<string, string> headers)
        {
            if (provider != PaymentMethod.Stripe)
                throw new InvalidOperationException("Invalid provider for Stripe gateway");

            if (!headers.TryGetValue("Stripe-Signature", out var signature))
                return new PaymentWebhookResult { IsValid = false };

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    _stripeSettings.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while parsing Stripe webhook: {Message}", ex.Message);
                return new PaymentWebhookResult { IsValid = false };
            }

            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return new PaymentWebhookResult { IsValid = false };

            return new PaymentWebhookResult
            {
                IsValid = true,
                Provider = PaymentMethod.Stripe,
                ProviderReferenceId = intent.Id,
                Status = MapStatus(intent.Status),
                Amount = intent.AmountReceived / 100m,
                OrderId = intent.Metadata.TryGetValue("OrderId", out var OrderId) && Guid.TryParse(OrderId, out var orderId) ? orderId : null,
                ClientId = intent.Metadata.TryGetValue("ClientId", out var ClientId)? ClientId : null,
            };
        }

        private static PaymentStatus MapStatus(string stripeStatus) =>
            stripeStatus switch
            {
                "succeeded" => PaymentStatus.Completed,
                "canceled" => PaymentStatus.Failed,
                "requires_payment_method" => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };

     
    }
}
