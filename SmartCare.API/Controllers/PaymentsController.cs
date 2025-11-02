// API/Controllers/PaymentsController.cs
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.IServices;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentGetway _paymentGateway;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentGetway paymentGateway,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _paymentService = paymentService;
            _paymentGateway = paymentGateway;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];
            var webhookSecret = _config["StripeSettings:WebhookSecret"];

            if (!_paymentGateway.VerifyWebhookSignature(json, signature, webhookSecret!, out var webhookEvent))
                return BadRequest("Invalid signature");

            await _paymentService.HandleWebhookEventAsync(webhookEvent);

            return Ok();
        }

        [HttpPost("success/{orderId}")]
        public async Task<IActionResult> Success(Guid orderId)
        {
            var result = await _paymentService.MarkPaymentSuccessAsync(orderId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpPost("fail/{orderId}")]
        public async Task<IActionResult> Fail(Guid orderId)
        {
            var result = await _paymentService.MarkPaymentFailureAsync(orderId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpPost("process/{orderId}")]
        public async Task<IActionResult> ProcessPayment(Guid orderId)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var returnUrl = $"{baseUrl}/payment";
            var requestdto = new CreateCheckoutSessionRequest
            {
                OrderId = orderId,
                ReturnUrl = returnUrl
            };
            var result = await _paymentService.ProcessPaymentAsync(requestdto);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}