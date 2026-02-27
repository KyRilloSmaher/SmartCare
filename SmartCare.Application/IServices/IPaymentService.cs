
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using Stripe;

namespace SmartCare.Application.IServices
{
    /// <summary>
    /// Handles all payment operations including online, offline,
    /// refunds, cancellations, and webhook processing.
    /// </summary>
    public interface IPaymentService
    {
        //Task<Response<SessionResponse>> ProcessPaymentAsync(CreateCheckoutSessionRequest request);

        //Task<Response<PaymentResult>> MarkPaymentSuccessAsync(Guid orderId);

        //Task<Response<PaymentResult>> MarkPaymentFailureAsync(Guid orderId);

        //Task<Response<PaymentResult>> TryCancelOrRefundAsync(Guid orderId);

        //Task HandleExpiredPaymentAsync(Guid orderId);



        Task HandleWebhookEventAsync(Event stripeEvent);
        Task<Response<PaymentResponseDTO>> CreateOrUpdatePaymentAsync(Guid orderId);
        Task<Response<PaymentResponseDTO>> PayOfflineAsync(string orderCode);
        Task<Response<bool>> MarkOrderPaymentAsCash(Guid OrderId);
        Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
    }
}

