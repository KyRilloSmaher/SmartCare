using MediatR;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.Application.CQRs.Payments.Commands.PayOfflineCommand
{
    public record PayOfflineCommand(string orderCode) : IRequest<Response<PaymentResponseDTO>>;
}
