
using SmartCare.Application.DTOs.Payment;
using SmartCare.Domain.Enums;

namespace SmartCare.Application.ExternalServiceInterfaces.Payments
{
    public interface IPaymentGetway
    {
        PaymentMethod Provider { get; }
        Task<PaymentSessionResult> CreateSessionAsync(CreatePaymentSessionCommand command, CancellationToken cancellationToken = default);

        Task CancelSessionAsync(string providerReferenceId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Parses incoming webhook payload from any payment provider.
        /// </summary>
        /// <param name="provider">The payment provider (Stripe, PayPal, etc.)</param>
        /// <param name="payload">Raw request body</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Result containing validation status and any error message</returns>
        PaymentWebhookResult ParseWebhook(PaymentMethod provider, string payload, IReadOnlyDictionary<string, string> headers);
    }
}

