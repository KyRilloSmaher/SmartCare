using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class HandleWebhookEventHandler : IRequestHandler<HandleWebhookEventAsyncCommand, Unit>
    {
        private readonly ILogger<HandleWebhookEventHandler> _logger;
        private readonly IMediator _mediator;

        public HandleWebhookEventHandler(ILogger<HandleWebhookEventHandler> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(HandleWebhookEventAsyncCommand request, CancellationToken cancellationToken)
        {
            var stripeEvent = request.stripeEvent;
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await _mediator.Send(new HandlePaymentIntentSucceededAsyncCommand(stripeEvent), cancellationToken);
                    break;

                case "payment_intent.payment_failed":
                    await _mediator.Send(new HandlePaymentIntentFailedAsyncCommand(stripeEvent), cancellationToken);
                    break;

                default:
                    _logger.LogInformation(
                        "Unhandled Stripe event: {EventType}", stripeEvent.Type);
                    break;
            }
            return Unit.Value;
        }
    }
}