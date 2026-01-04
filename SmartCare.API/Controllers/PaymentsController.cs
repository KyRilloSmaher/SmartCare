using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.IServices;
using Stripe;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
        }

        // ------------------------------------------------------
        // ONLINE PAYMENT (PAYMENT INTENT)
        // ------------------------------------------------------

        /// <summary>
        /// Creates or updates a PaymentIntent for an order.
        /// Frontend uses returned client_secret with Stripe Elements.
        /// </summary>
        [HttpPost("intent/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent(Guid orderId)
        {
            var result = await _paymentService.CreateOrUpdatePaymentAsync(orderId);
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
            var result = await _paymentService.MarkOrderPaymentAsCash(orderId);
            return ControllersHelperMethods.FinalResponse(result);
        }
        // ------------------------------------------------------
        // STRIPE WEBHOOK (SOURCE OF TRUTH)
        // ------------------------------------------------------
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhookAsync()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];
            var secret = _configuration["StripeSettings:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    secret!,
                    throwOnApiVersionMismatch: false
                );

                await _paymentService.HandleWebhookEventAsync(stripeEvent);
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Invalid Stripe webhook signature");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled webhook processing error");
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
            var result = await _paymentService.PayOfflineAsync(pickupCode);
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
