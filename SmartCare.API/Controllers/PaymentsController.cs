// API/Controllers/PaymentsController.cs
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Route("api/Payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentGetway _paymentGateway;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentGetway paymentGateway,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _paymentGateway = paymentGateway;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];
            var webhookSecret = _config["StripeSettings:WebhookSecret"];
            _logger.LogInformation("Webhook received: {Json}", json);
            _logger.LogInformation("Signature: {Signature}", signature);
            _logger.LogInformation("Webhook Secret: {WebhookSecret}", webhookSecret);

            Stripe.Event stripeEvent;

            try
            {
                // Verify signature and handle API version mismatch
                stripeEvent = Stripe.EventUtility.ConstructEvent(
                    json,
                    signature,
                    webhookSecret!,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (Stripe.StripeException)
            {
                Console.WriteLine("Invalid Stripe webhook signature");
                return BadRequest("Invalid signature");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Stripe webhook: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }

            try
            {
                //only process important events
                if (stripeEvent.Type == "payment_intent.succeeded" || stripeEvent.Type == "charge.succeeded"|| stripeEvent.Type == "payment_intent.payment_failed" || stripeEvent.Type =="charge.failed")

                {
                    await _paymentService.HandleWebhookEventAsync(stripeEvent);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process Stripe webhook event: {stripeEvent.Type}, Error: {ex.Message}");
                return StatusCode(500);
            }

            return Ok();
        }

        [HttpPost("process/{orderId}")]
        public async Task<IActionResult> ProcessPayment(Guid orderId)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var returnUrl = $"{baseUrl}/api/Payments";
            var requestdto = new CreateCheckoutSessionRequest
            {
                OrderId = orderId,
                ReturnUrl = returnUrl
            };
            var result = await _paymentService.ProcessPaymentAsync(requestdto);

            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpGet("stripe-session")]
        public async Task<IActionResult> Session([FromQuery]Guid orderId)
        {
            var session = await _paymentService.GetPaymentByOrderIdAsync(orderId);

            if (session == null)
                return NotFound();

            if (DateTime.UtcNow > session.ExpiredAt)
                return BadRequest("Payment link has expired.");

            return Redirect(session.url);
        }


        [HttpGet("success/{orderId}")]
        public async Task<IActionResult> Success(Guid orderId)
        {
            var result = await _paymentService.MarkPaymentSuccessAsync(orderId);
            if (result.Succeeded)
            {
                return Content(SystemMessages.PaymentSuccessPage, "text/html");
            }
            return Content(SystemMessages.PaymentFailurePage, "text/html");
        }

        [HttpGet("fail/{orderId}")]
        public async Task<IActionResult> Fail(Guid orderId)
        {
            var result = await _paymentService.MarkPaymentFailureAsync(orderId);
            //return ControllersHelperMethods.FinalResponse(result);
            return Content(SystemMessages.PaymentFailurePage, "text/html");
        }

       
    }
}