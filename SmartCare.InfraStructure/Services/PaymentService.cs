using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.payment;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.Repositories;
using Stripe;
using Stripe.Checkout;
using System.Security.Cryptography;
using System.Text;

namespace SmartCare.InfraStructure.Services
{
    /// <summary>
    /// Implements payment processing logic for online and offline orders.
    /// Handles Stripe checkout, webhooks, refunds, cancellations, and expiration.
    /// </summary>
    public sealed class PaymentService : IPaymentService
    {
        private readonly IPaymentGetway _paymentGateway;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentService> _logger;
        private readonly ICartRepository _cartRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly int _paymentExpirationMinutes;

        public PaymentService(
            IPaymentGetway paymentGateway,
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IInventoryRepository inventoryRepository,
            IClientRepository clientRepository,
            IReservationRepository reservationRepository,
            IBackgroundJobService backgroundJobs,
            IResponseHandler responseHandler,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IConfiguration configuration,
            ICartRepository cartRepository,
            IEmailService emailService,
            IStoreRepository storeRepository)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _inventoryRepository = inventoryRepository;
            _clientRepository = clientRepository;
            _reservationRepository = reservationRepository;
            _backgroundJobs = backgroundJobs;
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;

            _paymentExpirationMinutes =
                configuration.GetValue<int?>("ReservationTimes:ForPaymentExpirationMinutes") ?? 10;
            _cartRepository = cartRepository;
            _emailService = emailService;
            _storeRepository = storeRepository;
        }
        #region PaymnetIntent Version 
        public async Task<Response<PaymentIntentResponse>> CreateOrUpdatePaymentAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
                return _responseHandler.BadRequest<PaymentIntentResponse>("Order not found");

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<PaymentIntentResponse>("Order not payable");

            PaymentIntent intent;

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                intent = await _paymentGateway.CreatePaymentIntentAsync(
                    order.TotalPrice,
                    order.Id.ToString(),
                    order.PaymentVersion);

                order.PaymentIntentId = intent.Id;

                await _paymentRepository.AddAsync(new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalPrice,
                    PaymentIntentId = intent.Id,
                    ClientSecret = intent.ClientSecret,
                    Version = order.PaymentVersion
                });
            }
            else
            {
                intent = await _paymentGateway.UpdatePaymentIntentAmountAsync(
                    order.PaymentIntentId,
                    order.TotalPrice);
            }

            await _orderRepository.UpdateAsync(order);


            return _responseHandler.Success(new PaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = order.TotalPrice
            });
        }
        private async Task HandlePaymentIntentSucceededAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return;

            //  Read metadata
            if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                !intent.Metadata.TryGetValue("version", out var versionStr))
                return;

            if (!Guid.TryParse(orderIdStr, out var orderId) ||
                !int.TryParse(versionStr, out var version))
                return;

            // Load order
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null) return;

            // HARD SECURITY CHECKS 
            if (order.PaymentIntentId != intent.Id) return;
            if (order.PaymentVersion != version) return;
            var paidAmount = intent.Amount / 100m;
            if (decimal.Round(order.TotalPrice, 2) != decimal.Round(paidAmount, 2))
                return;

            if (order.Status != OrderStatus.Pending) return;

            // Mark order as paid
            order.Status = OrderStatus.Confirmed;

            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (payment == null) return;

            payment.Status = PaymentStatus.Completed;


            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                try
                {
                    await _inventoryRepository.FinalizeStockDeductionAsync(
                        item.InvetoryId,
                        item.Quantity,
                        order is FromStoreOrder
                    );
                    await _reservationRepository.UpdateReservationStatusAsync(
                        (Guid)item.ReservationId,
                        ReservationStatus.Completed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Inventory finalization failed. Order {OrderId}, Inventory {InventoryId}",
                        order.Id, item.InvetoryId);
                }
            }

            await IncrementClientOrdersAsync(order.ClientId);
            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);
            ////  Clear cart
            var cart = await _cartRepository.GetActiveCartAsync(order.ClientId);
            await _cartRepository.DeleteAsync(cart);
            await _cartRepository.CreateCartAsync(order.ClientId);

            PublishPaymentEvent(order, "success", "Payment completed successfully.");
            var client = await _clientRepository.GetByIdAsync(order.ClientId);
            if (order.OrderType == OrderType.Online)
            {
                await SendOrderConfirmationEmailAsync(order, client);
            }
            else
            {
                var pickupCode = RandomNumberGenerator
                                    .GetInt32(0, 1_000_000)
                                    .ToString("D7");

                await _orderRepository.UpdatePickupCodeHashAsync(
                    order.Id,
                    ComputeSha256(pickupCode));
                await SendPickupEmailAsync(order, client, pickupCode, ((FromStoreOrder)order).StoreId);
            }

             
        }
        private async Task HandlePaymentIntentFailedAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return;

            if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                !Guid.TryParse(orderIdStr, out var orderId))
                return;

            var order = await _orderRepository.GetByIdAsync(orderId, true);
            if (order == null) return;

            if (order.Status != OrderStatus.Pending) return;

            order.Status = OrderStatus.PaymentFailed;

            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (payment == null) return;

            payment.Status = PaymentStatus.Completed;


            await _orderRepository.UpdateAsync(order);
            await _paymentRepository.UpdateAsync(payment);

            PublishPaymentEvent(order, "failed", "Payment failed");
        }
        public async Task HandleWebhookEventAsync(Event stripeEvent)
        {
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceededAsync(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailedAsync(stripeEvent);
                    break;

                default:
                    _logger.LogInformation(
                        "Unhandled Stripe event: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        public async Task<Response<bool>> MarkOrderPaymentAsCash(Guid OrderId )
        {
            var order = await _orderRepository.GetByIdAsync(OrderId, true);
            if (order == null) return _responseHandler.Failed<bool>(SystemMessages.ORDER_NOT_FOUND);

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<bool>("Order is not payable.");
            order.Status = OrderStatus.Confirmed;
            // ToDo : Set Payment Cash Case
            await _orderRepository.UpdateAsync(order);
            var client = await _clientRepository.GetByIdAsync(order.ClientId);
            if (order.OrderType == OrderType.Online)
            {
                await SendOrderConfirmationEmailAsync(order, client);
            }
            else
            {
                var pickupCode = RandomNumberGenerator
                                    .GetInt32(0, 1_000_000)
                                    .ToString("D7");

                await _orderRepository.UpdatePickupCodeHashAsync(
                    order.Id,
                    ComputeSha256(pickupCode));
                await SendPickupEmailAsync(order, client, pickupCode, ((FromStoreOrder)order).StoreId);
            }
            return _responseHandler.Success(true);
        }

        #endregion
        #region Online Payments (Session Version)

        //public async Task<Response<SessionResponse>> ProcessPaymentAsync(CreateCheckoutSessionRequest request)
        //{
        //    var order = await _orderRepository.GetByIdAsync(request.OrderId, true);
        //    if (order is null)
        //        return _responseHandler.BadRequest<SessionResponse>(SystemMessages.ORDER_NOT_FOUND);

        //    if (order.Status != OrderStatus.Pending)
        //        return _responseHandler.BadRequest<SessionResponse>(SystemMessages.CAN_NOT_PROCESS_PAYMENT);

        //    var sessionRequest = new PaymentSessionRequest
        //    {
        //        Amount = order.TotalPrice,
        //        OrderId = order.Id.ToString(),
        //        SuccessUrl = $"{request.ReturnUrl}/success/{order.Id}",
        //        CancelUrl = $"{request.ReturnUrl}/fail/{order.Id}"
        //    };

        //    var session = await _paymentGateway.CreateCheckoutSessionAsync(sessionRequest);

        //    var payment = _mapper.Map<Payment>(session);
        //    payment.OrderId = order.Id;
        //    payment.Status = PaymentStatus.Pending;
        //    payment.ExpiredAt = DateTime.UtcNow.AddMinutes(_paymentExpirationMinutes);
        //    payment.url = session.Url;

        //    await _paymentRepository.AddAsync(payment);

        //    order.PaymentId = payment.Id;
        //    await _orderRepository.UpdateAsync(order);

        //    return _responseHandler.Success(new SessionResponse
        //    {
        //        Id = session.Id,
        //        url = $"{request.ReturnUrl}/stripe-session?orderId={order.Id}"
        //    });
        //}

        //public async Task HandleWebhookEventAsync(Event stripeEvent)
        //{
        //    switch (stripeEvent.Type)
        //    {
        //        case "checkout.session.completed":
        //            await HandleCheckoutCompletedAsync(stripeEvent);
        //            break;

        //        case "payment_intent.payment_failed":
        //            await HandlePaymentFailedAsync(stripeEvent);
        //            break;

        //        default:
        //            _logger.LogInformation("Unhandled Stripe event: {Type}", stripeEvent.Type);
        //            break;
        //    }
        //}

        //public async Task<Response<PaymentResult>> MarkPaymentSuccessAsync(Guid orderId)
        //{
        //    var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
        //    if (order is null)
        //        return _responseHandler.BadRequest<PaymentResult>("Order not found.");

        //    var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        //    if (payment is null)
        //        return _responseHandler.BadRequest<PaymentResult>("Payment not found.");

        //    order.Status = OrderStatus.Confirmed;
        //    payment.Status = PaymentStatus.Completed;

        //    foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
        //    {
        //        try
        //        {
        //            await _inventoryRepository.FinalizeStockDeductionAsync(
        //                item.InvetoryId,
        //                item.Quantity,
        //                order is FromStoreOrder
        //            );
        //            await _reservationRepository.UpdateReservationStatusAsync(
        //                (Guid)item.ReservationId,
        //                ReservationStatus.Completed);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex,
        //                "Inventory finalization failed. Order {OrderId}, Inventory {InventoryId}",
        //                order.Id, item.InvetoryId);
        //        }
        //    }

        //    await IncrementClientOrdersAsync(order.ClientId);
        //    await _orderRepository.UpdateAsync(order);
        //    await _paymentRepository.UpdateAsync(payment);
        //    ////  Clear cart
        //    var cart = await _cartRepository.GetActiveCartAsync(order.ClientId);
        //    await _cartRepository.DeleteAsync(cart);
        //    await _cartRepository.CreateCartAsync(order.ClientId);

        //    PublishPaymentEvent(order, "success", "Payment completed successfully.");

        //    return _responseHandler.Success(
        //        new PaymentResult(true, SystemMessages.PAYMENT_PROCESSED, payment.SessionId)
        //    );
        //}

        //public async Task<Response<PaymentResult>> MarkPaymentFailureAsync(Guid orderId)
        //{
        //    var order = await _orderRepository.GetByIdAsync(orderId, true);
        //    if (order is null)
        //        return _responseHandler.BadRequest<PaymentResult>("Order not found.");

        //    var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        //    if (payment is null)
        //        return _responseHandler.BadRequest<PaymentResult>("Payment not found.");

        //    order.Status = OrderStatus.PaymentFailed;
        //    payment.Status = PaymentStatus.Failed;

        //    await _orderRepository.UpdateAsync(order);
        //    await _paymentRepository.UpdateAsync(payment);

        //    PublishPaymentEvent(order, "failed", "Payment failed or canceled.");

        //    return _responseHandler.Failed<PaymentResult>(SystemMessages.PAYMENT_FAILED);
        //}

        #endregion

        #region Refunds & Expiration

        //public async Task<Response<PaymentResult>> TryCancelOrRefundAsync(Guid orderId)
        //{
        //    var order = await _orderRepository.GetByIdAsync(orderId, true);
        //    if (order is null)
        //        return _responseHandler.BadRequest<PaymentResult>(SystemMessages.ORDER_NOT_FOUND);

        //    var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        //    if (payment is null)
        //        return _responseHandler.BadRequest<PaymentResult>(SystemMessages.NOT_FOUND);

        //    if (payment.Status == PaymentStatus.Pending)
        //    {
        //        payment.Status = PaymentStatus.Failed;
        //        order.Status = OrderStatus.Cancelled;
        //    }
        //    else if (payment.Status == PaymentStatus.Completed)
        //    {
        //        if (!await _paymentGateway.RefundPaymentAsync(payment.SessionId))
        //            return _responseHandler.BadRequest<PaymentResult>("Refund failed.");

        //        payment.Status = PaymentStatus.Refunded;
        //        order.Status = OrderStatus.Refunded;
        //    }
        //    else
        //    {
        //        return _responseHandler.BadRequest<PaymentResult>(
        //            $"Cannot process payment in '{payment.Status}' state.");
        //    }

        //    await _paymentRepository.UpdateAsync(payment);
        //    await _orderRepository.UpdateAsync(order);

        //    PublishPaymentEvent(order, payment.Status.ToString().ToLower(), "Payment updated.");

        //    return _responseHandler.Success(
        //        new PaymentResult(true, "Payment updated successfully.", payment.SessionId)
        //    );
        //}

        //public async Task HandleExpiredPaymentAsync(Guid orderId)
        //{
        //    var order = await _orderRepository.GetByIdAsync(orderId, true);
        //    if (order is null || order.Status != OrderStatus.Pending)
        //        return;

        //    await ReleaseReservationsAsync(orderId);

        //    order.Status = OrderStatus.Expired;
        //    await _orderRepository.UpdateAsync(order);

        //    if (order.PaymentId != 0)
        //        await TryCancelOrRefundAsync(order.Id);
        //}

        #endregion

        #region Offline Payment

        public async Task<Response<PaymentResult>> PayOfflineAsync(string orderCode)
        {
            var hashedCode = ComputeSha256(orderCode);

            // 1. Get the order by pickup code
            var order = await _orderRepository.GetOrderByPickUpCode(hashedCode);
            if (order is null)
                return _responseHandler.BadRequest<PaymentResult>(SystemMessages.ORDER_NOT_FOUND);

            // 2. Only pending orders can be paid offline
            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<PaymentResult>("Order is not payable.");

            // 3. Check if a payment already exists
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(order.Id);

            if (existingPayment != null)
            {
                // Update existing payment
                existingPayment.Amount = order.TotalPrice;
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.Method = Domain.Enums.PaymentMethod.Cash;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(existingPayment);
            }
            else
            {
                // Create new offline payment
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Status = PaymentStatus.Completed,
                    Amount = order.TotalPrice,
                    PaymentIntentId = null,
                    CreatedAt = DateTime.UtcNow,
                    Method = Domain.Enums.PaymentMethod.Cash
                };
                await _paymentRepository.AddAsync(payment);
                order.PaymentIntentId = payment.Id.ToString();
            }

            // 4. Update order
            order.Status = OrderStatus.Completed;
            await _orderRepository.UpdateAsync(order);

            // 5. Finalize inventory and reservations
            _backgroundJobs.Enqueue(() => FinishReservationsAsync(order.Id));

            // 6. Increment client stats & publish event
            await IncrementClientOrdersAsync(order.ClientId);
            PublishPaymentEvent(order, "success", "Offline payment completed successfully.");

            return _responseHandler.Success(
                new PaymentResult(true, SystemMessages.PAYMENT_PROCESSED, hashedCode)
            );
        }

        public Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId)
            => _paymentRepository.GetByOrderIdAsync(orderId);

        #endregion

        #region Helpers
        private  string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }
        private async Task IncrementClientOrdersAsync(string clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, true);
            if (client == null) return;

            client.OrdersCount++;
            await _clientRepository.UpdateAsync(client);
        }

        private async Task ReleaseReservationsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order is null) return;

            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                if (item.ReservationId == Guid.Empty) continue;

                await _reservationRepository.CancelReservationAsync(
                    (Guid)item.ReservationId,
                    item.InvetoryId,
                    ReservationStatus.PaymentTimeOut);
            }
        }
        private async Task FinishReservationsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order is null) return;
            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                await _inventoryRepository.FinalizeStockDeductionAsync(
                        item.InvetoryId,
                        item.Quantity,
                        order is FromStoreOrder
                    );
                if (item.ReservationId == Guid.Empty) continue;
                   await _reservationRepository.UpdateReservationStatusAsync((Guid)item.ReservationId,ReservationStatus.Completed);
            }
        }

        private void PublishPaymentEvent(Order order, string status, string message)
        {
            _backgroundJobs.Enqueue<IEventPublisherService>(publisher =>
                publisher.PublishPaymentStatusChanged(order.Id, order.ClientId, status, message));
        }

        //private async Task HandleCheckoutCompletedAsync(Event stripeEvent)
        //{
        //    if (stripeEvent.Data.Object is not Session session) return;

        //    if (session.Metadata.TryGetValue("orderId", out var id) &&
        //        Guid.TryParse(id, out var orderId))
        //    {
        //        await MarkPaymentSuccessAsync(orderId);
        //    }
        //}

        //private async Task HandlePaymentFailedAsync(Event stripeEvent)
        //{
        //    if (stripeEvent.Data.Object is not PaymentIntent intent) return;

        //    if (intent.Metadata.TryGetValue("orderId", out var id) &&
        //        Guid.TryParse(id, out var orderId))
        //    {
        //        await MarkPaymentFailureAsync(orderId);
        //    }
        //}
        private async Task SendPickupEmailAsync(Order order, Client client, string pickupCode, Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);

            var emailBody = SystemMessages.PICKUP_ORDER_EMAIL_TEMPLATE
                .Replace("{{UserName}}", client.UserName)
                .Replace("{{PickupCode}}", pickupCode)
                .Replace("{{StoreName}}", store.Name)
                .Replace("{{StoreAddress}}", store.Address)
                .Replace("{{OrderDate}}", order.CreatedAt.ToString("MMMM dd, yyyy"))
                .Replace("{{OrderTotal}}", order.TotalPrice.ToString())
                .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            _backgroundJobs.Schedule(
                () => _emailService.SendEmailAsync(
                    client.Email,
                    "Your Pickup Order Details",
                    emailBody),
                TimeSpan.FromSeconds(5));
        }
        private async Task SendOrderConfirmationEmailAsync(Order order, Client client)
        {

            var emailBody = SystemMessages.ORDERCONFIRMATION_TEMPLATE
                       .Replace("{{UserName}}", client.UserName)
                       .Replace("{{OrderId}}", order.Id.ToString("N")[^6..])
                       .Replace("{{OrderDate}}", order.CreatedAt.ToString("MMMM dd, yyyy"))
                       .Replace("{{OrderTotal}}", order.TotalPrice.ToString())
                       .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            _backgroundJobs.Schedule(() => _emailService.SendEmailAsync(client.Email, "Your  Order Details", emailBody),
                                            TimeSpan.FromSeconds(5));
;
        }
        #endregion
    }
}
