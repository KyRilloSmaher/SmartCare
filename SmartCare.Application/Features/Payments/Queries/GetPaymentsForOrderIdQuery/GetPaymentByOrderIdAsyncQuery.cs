using MediatR;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.DTOs.Payment;

namespace SmartCare.Application.CQRs.Payments.Queries.GetPaymentsForOrderIdQuery
{
    public record GetPaymentsByOrderIdQuery(Guid orderId) : IRequest<Response<IEnumerable<PaymentResponseDTO>>>;
}
