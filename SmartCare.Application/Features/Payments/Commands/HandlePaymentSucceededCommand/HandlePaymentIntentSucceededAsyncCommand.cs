using MediatR;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payments.Commands.HandlePaymentSucceededCommand
{
    public record HandlePaymentSucceededAsyncCommand(PaymentWebhookResult paymentwebHookEventResult) : IRequest<Response<bool>>;
}
