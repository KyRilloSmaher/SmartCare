using MediatR;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Commands
{
    public record HandlePaymentIntentSucceededAsyncCommand(Event stripeEvent) : IRequest<Unit>;
}
