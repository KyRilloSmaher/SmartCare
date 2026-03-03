
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.ExternalServices.Payments
{
    public class PaymobService : IPaymentGetway
    {
        public PaymentMethod Provider => PaymentMethod.Paymob;

        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobService> _logger;
        private readonly HttpClient _httpClient;

        public PaymobService(IOptions<PaymobSettings> settings, ILogger<PaymobService> logger, HttpClient httpClient)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PaymentSessionResult> CreateSessionAsync(CreatePaymentSessionCommand command, CancellationToken cancellationToken = default)
        {
            // 1️⃣ Authenticate with Paymob to get a token
            var authResponse = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/auth/tokens",
                new { api_key = _settings.ApiKey },
                cancellationToken
            );

            var authJson = await authResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            string token = authJson.GetProperty("token").GetString();

            // 2️⃣ Create an order
            var orderRequest = new
            {
                auth_token = token,
                amount_cents = (int)(command.Amount * 100),
                currency = command.Currency.ToUpper(),
                items = new object[] { },
                merchant_order_id = command.OrderId.ToString()
            };

            var orderResponse = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/ecommerce/orders",
                orderRequest,
                cancellationToken
            );

            var orderJson = await orderResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            int orderId = orderJson.GetProperty("id").GetInt32();

            // 3️⃣ Get a payment key
            var paymentKeyRequest = new
            {
                auth_token = token,
                amount_cents = (int)(command.Amount * 100),
                expiration = 3600,
                order_id = orderId,
                integration_id = _settings.IntegrationId,
            };

            var paymentKeyResponse = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/acceptance/payment_keys",
                paymentKeyRequest,
                cancellationToken
            );

            var paymentKeyJson = await paymentKeyResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            string paymentToken = paymentKeyJson.GetProperty("token").GetString();

            return new PaymentSessionResult
            {
                Provider = PaymentMethod.Paymob,
                ProviderReferenceId = orderId.ToString(),
                ClientPaymentToken = paymentToken
            };
        }

        public Task CancelSessionAsync(string providerReferenceId, CancellationToken cancellationToken = default)
        {
            // Paymob does not provide direct session cancel; optionally implement refunds here
            _logger.LogWarning("CancelSessionAsync is not implemented for Paymob.");
            return Task.CompletedTask;
        }

        public PaymentWebhookResult ParseWebhook(PaymentMethod provider, string payload, IReadOnlyDictionary<string, string> headers)
        {
            if (provider != PaymentMethod.Paymob)
                throw new InvalidOperationException("Invalid provider for Paymob gateway");

            try
            {
                var json = JsonDocument.Parse(payload).RootElement;

                // Optionally verify secret header if Paymob provides one
                if (headers.TryGetValue("X-Paymob-Signature", out var signature))
                {
                    if (!string.IsNullOrEmpty(_settings.WebhookSecret) && signature != _settings.WebhookSecret)
                    {
                        _logger.LogWarning("Paymob webhook signature mismatch.");
                        return new PaymentWebhookResult { IsValid = false };
                    }
                }

                bool success = json.GetProperty("success").GetBoolean();
                int amountCents = json.GetProperty("amount_cents").GetInt32();
                decimal amount = amountCents / 100m;

                var orderElement = json.GetProperty("order");
                string providerOrderId = orderElement.GetProperty("id").GetInt32().ToString();

                Guid? orderId = null;
                string merchantOrderId = orderElement.GetProperty("merchant_order_id").GetString();
                if (Guid.TryParse(merchantOrderId, out var guid))
                    orderId = guid;

                return new PaymentWebhookResult
                {
                    IsValid = true,
                    Provider = PaymentMethod.Paymob,
                    ProviderReferenceId = providerOrderId,
                    Status = success ? PaymentStatus.Completed : PaymentStatus.Failed,
                    Amount = amount,
                    OrderId = orderId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Paymob webhook payload");
                return new PaymentWebhookResult { IsValid = false };
            }
        }
    }
}