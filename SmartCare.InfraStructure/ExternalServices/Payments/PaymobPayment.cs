using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public PaymobService(
            IOptions<PaymobSettings> settings,
            ILogger<PaymobService> logger,
            HttpClient httpClient)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PaymentSessionResult> CreateSessionAsync(CreatePaymentSessionCommand command,CancellationToken cancellationToken = default)
        {
            var billingData = new
            {
                apartment = "NA",
                first_name = "Guest",
                last_name = "User",
                street = "NA",
                building = "NA",
                phone_number = "01000000000",
                country = "EG",
                email = "guest@test.com",
                floor = "NA",
                state = "Cairo",
                city = "Cairo"
            };

            var payload = new
            {
                amount = (int)(command.Amount * 100),
                currency = command.Currency?.ToUpper() ?? "EGP",
                payment_methods = new[] { _settings.IntegrationId },
                billing_data = billingData,

                items = new[]
                {
                    new
                    {
                        name = "Test Product",
                        amount = (int)(command.Amount * 100),
                        quantity = 1
                    }
                },

                customer = new
                {
                    first_name = billingData.first_name,
                    last_name = billingData.last_name,
                    email = billingData.email
                },

                special_reference = $"{command.OrderId}",
                expiration = 3600,
                merchantOrderId = $"{command.OrderId}"
            };
            // Create HTTP request for Paymob's intention API
            var requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Token", _settings.SecretKey);
            requestMessage.Content = JsonContent.Create(payload);
            // Send the request and process response
            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paymob Intention API call failed with status {response.StatusCode}: {responseContent}");
            }
            _logger.LogDebug($"Response :{response}");
            // Parse the response to get client_secret
            var resultJson = JsonDocument.Parse(responseContent);
            var clientSecret = resultJson.RootElement.GetProperty("client_secret").GetString();

            return new PaymentSessionResult
            {
                Provider = PaymentMethod.Paymob,
                ProviderReferenceId = command.OrderId.ToString(),
                ClientPaymentToken = clientSecret
            };
        }

        public Task CancelSessionAsync(
            string providerReferenceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("CancelSessionAsync is not implemented for Paymob.");
            return Task.CompletedTask;
        }

        public PaymentWebhookResult ParseWebhook(PaymentMethod provider,string payload,IReadOnlyDictionary<string, string> headers)
        {
            if (provider != PaymentMethod.Paymob)
                throw new InvalidOperationException("Invalid provider for Paymob gateway");

            try
            {
                var root = JsonDocument.Parse(payload).RootElement;

                // Signature validation (optional)
                if (headers.TryGetValue("X-Paymob-Signature", out var signature))
                {
                    if (!string.IsNullOrEmpty(_settings.WebhookSecret) &&
                        signature != _settings.WebhookSecret)
                    {
                        _logger.LogWarning("Paymob webhook signature mismatch.");
                        return new PaymentWebhookResult { IsValid = false };
                    }
                }

                // Paymob wraps transaction inside "obj"
                if (!root.TryGetProperty("obj", out var obj))
                {
                    _logger.LogWarning("Paymob webhook missing 'obj'.");
                    return new PaymentWebhookResult { IsValid = false };
                }

                bool success = obj.TryGetProperty("success", out var successEl) && successEl.GetBoolean();
                bool pending = obj.TryGetProperty("pending", out var pendingEl) && pendingEl.GetBoolean();
                bool refunded = obj.TryGetProperty("is_refunded", out var refundEl) && refundEl.GetBoolean();
                bool voided = obj.TryGetProperty("is_voided", out var voidEl) && voidEl.GetBoolean();

                int amountCents = obj.TryGetProperty("amount_cents", out var amountEl)
                    ? amountEl.GetInt32()
                    : 0;

                decimal amount = amountCents / 100m;

                if (!obj.TryGetProperty("order", out var orderElement))
                {
                    _logger.LogWarning("Paymob webhook missing order.");
                    return new PaymentWebhookResult { IsValid = false };
                }

                string providerOrderId = orderElement.TryGetProperty("id", out var idEl)
                    ? idEl.GetInt32().ToString()
                    : string.Empty;

                Guid? orderId = null;

                if (orderElement.TryGetProperty("merchant_order_id", out var merchantIdEl))
                {
                    var merchantOrderId = merchantIdEl.GetString();

                    if (Guid.TryParse(merchantOrderId, out var parsed))
                        orderId = parsed;
                }

                PaymentStatus status;

                if (pending)
                    status = PaymentStatus.Pending;
                else if (refunded)
                    status = PaymentStatus.Refunded;
                else if (voided)
                    status = PaymentStatus.Cancelled;
                else if (success)
                    status = PaymentStatus.Completed;
                else
                    status = PaymentStatus.Failed;

                return new PaymentWebhookResult
                {
                    IsValid = true,
                    Provider = PaymentMethod.Paymob,
                    ProviderReferenceId = orderId.ToString()/*providerOrderId*/,
                    Status = status,
                    Amount = amount,
                    OrderId = orderId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Paymob webhook payload: {Payload}", payload);
                return new PaymentWebhookResult { IsValid = false };
            }
        }
    }
}