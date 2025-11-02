using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartCare.API.Hubs;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.IServices;
using SmartCare.Application.Services;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe;
using Stripe.Checkout;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentGetway _stripeService;
        private readonly IPaymentService _paymentService ;
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<PaymentsHub> _hubContext;
        private readonly IConfiguration _config;

        public PaymentsController(
            IPaymentGetway stripeService,
            IOrderRepository orderRepository,
            IHubContext<PaymentsHub> hubContext,
            IConfiguration config)
        {
            _stripeService = stripeService;
            _orderRepository = orderRepository;
            _hubContext = hubContext;
            _config = config;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];
            var webhookSecret = _config["StripeSettings:WebhookSecret"];

            if (!_stripeService.VerifyWebhookSignature(json, signature, webhookSecret!, out var stripeEvent))
                return BadRequest();

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null) break;

                    var order = await _orderRepository.GetByIdAsync(Guid.Parse(session.ClientReferenceId!), true);
                    if (order == null) break;

                    if (order.Status == OrderStatus.Completed) break;

                    order.Status = OrderStatus.Completed;
                    order.Payment.PaymentIntentId = session.PaymentIntentId;
                    await _orderRepository.UpdateAsync(order);

                    await _hubContext.Clients.Group($"user:{order.ClientId}")
                        .SendAsync("PaymentUpdated", new { orderId = order.Id, status = "Completed" });
                    break;

                case "payment_intent.payment_failed":
                    // You can handle failures here
                    break;
            }

            return Ok();
        }
        [HttpPost("success/{orderId}")]
        public async Task<IActionResult> Success(string orderId)
        {
            var response = await _paymentService.MarkPaymentSuccessAsync(orderId);
            return Ok(response);
        }

        [HttpPost("fail/{orderId}")]
        public async Task<IActionResult> Fail(string orderId)
        {
            var response = await _paymentService.MarkPaymentFailureAsync(orderId);
            return Ok(response);
        }
    }
}
