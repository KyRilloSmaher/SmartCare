using MediatR;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payments.Commands.RequestpaymentSession
{
    public record RequestpaymentSessionCommandHandler(PaymentMethod Provider ,Guid orderId) : IRequest<Response<PaymentSessionResult>>;
}
