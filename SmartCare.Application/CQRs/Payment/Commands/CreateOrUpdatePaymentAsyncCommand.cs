using MediatR;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Commands
{
    public record CreateOrUpdatePaymentAsyncCommand(Guid orderId) : IRequest<Response<PaymentIntentResponse>>;
}
