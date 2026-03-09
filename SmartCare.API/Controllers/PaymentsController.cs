using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Payments.Commands.HandlePaymentFailedCommand;
using SmartCare.Application.CQRs.Payments.Commands.HandlePaymentSucceededCommand;
using SmartCare.Application.CQRs.Payments.Commands.MarkOrderPaymentAsCashCommand0;
using SmartCare.Application.CQRs.Payments.Commands.PayOfflineCommand;
using SmartCare.Application.CQRs.Payments.Commands.RequestpaymentSession;
using SmartCare.Application.ExternalServiceInterfaces.Payments;

using SmartCare.Domain.Enums;


namespace SmartCare.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        public PaymentsController(IMediator mediator, IConfiguration configuration, ILogger<PaymentsController> logger, IPaymentGatewayFactory paymentGatewayFactory)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
            _paymentGatewayFactory = paymentGatewayFactory;
        }


        // ------------------------------------------------------
        // ONLINE PAYMENT (PAYMENT INTENT)
        // ------------------------------------------------------

        /// <summary>
        /// Creates or updates a PaymentIntent for an order.
        /// </summary>
        [HttpPost("{Provider}/Pruches/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent([FromRoute]PaymentMethod Provider,[FromRoute]Guid orderId)
        {
            //var result = await _paymentService.CreateOrUpdatePaymentAsync(orderId);
            var result = await _mediator.Send(new RequestpaymentSessionCommandHandler(Provider,orderId));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        ///  Mark Order As Cash Payment
        /// Frontend uses this when User Choose to Pay with Cash
        /// </summary>
        [HttpPost("mark-as-cash-payment/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> MarkAsCashPaymentAsync(Guid orderId)
        {
            //var result = await _paymentService.MarkOrderPaymentAsCash(orderId);
            var result = await _mediator.Send(new MarkOrderPaymentAsCashCommand(orderId));
            return ControllersHelperMethods.FinalResponse(result);
        }
        // ------------------------------------------------------
        // Payment WEBHOOK (SOURCE OF TRUTH)
        // ------------------------------------------------------
        [HttpPost("{provider}-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> WebhookAsync(string provider)
        {

            // Try parse provider dynamically
            if (!Enum.TryParse<PaymentMethod>(provider, true, out var paymentProvider))
            {
                _logger.LogWarning("Unknown payment provider webhook: {Provider}", provider);
                return BadRequest("Unknown provider");
            }

            var paymentService = _paymentGatewayFactory.Resolve(paymentProvider);

            _logger.LogInformation("Processing {Provider} webhook", paymentProvider);

            var payload = await new StreamReader(Request.Body).ReadToEndAsync();

            // Convert headers to dictionary
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            try
            {
                var result = paymentService.ParseWebhook(paymentProvider, payload, headers);

                if (!result.IsValid)
                {
                    _logger.LogWarning("Webhook validation failed: {Error}", result.Status);
                    return BadRequest(result.Status);
                }
                if (result.Status == PaymentStatus.Failed)
                {
                    var response = await _mediator.Send(new HandlePaymentIntentFailedAsyncCommand(result));
                    return ControllersHelperMethods.FinalResponse(response); ;
                }
                else if (result.Status == PaymentStatus.Completed)
                {
                    var response = await _mediator.Send(new HandlePaymentSucceededAsyncCommand(result));
                    return ControllersHelperMethods.FinalResponse(response);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled webhook processing error for {Provider}", paymentProvider);
                return Ok();
            }
        
        }


        // ------------------------------------------------------
        // OFFLINE PAYMENT (CASH / PICKUP)
        // ------------------------------------------------------

        [HttpPost("offline")]
        [Authorize(Roles = "Admin,Store")]
        public async Task<IActionResult> PayOfflineAsync([FromBody] string pickupCode)
        {
            //var result = await _paymentService.PayOfflineAsync(pickupCode);
            var result = await _mediator.Send(new PayOfflineCommand(pickupCode));
            return ControllersHelperMethods.FinalResponse(result);
        }

        // ------------------------------------------------------
        // CANCEL / REFUND
        // ------------------------------------------------------

        //[HttpPost("cancel-or-refund/{orderId:guid}")]
        //[Authorize]
        //public async Task<IActionResult> CancelOrRefundAsync(Guid orderId)
        //{
        //    var result = await _paymentService.TryCancelOrRefundAsync(orderId);
        //    return ControllersHelperMethods.FinalResponse(result);
        //}
    }
}
