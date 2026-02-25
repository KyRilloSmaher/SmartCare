using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class HandlePaymentIntentSucceededHandler : IRequestHandler<HandlePaymentIntentSucceededAsyncCommand, Unit>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<HandlePaymentIntentSucceededHandler> _logger;
        private readonly ICartRepository _cartRepository;
        private readonly PaymentExtensions _paymentExtensions;

        public HandlePaymentIntentSucceededHandler(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IInventoryRepository inventoryRepository, IClientRepository clientRepository, IReservationRepository reservationRepository, ILogger<HandlePaymentIntentSucceededHandler> logger, ICartRepository cartRepository, PaymentExtensions paymentExtensions)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _inventoryRepository = inventoryRepository;
            _clientRepository = clientRepository;
            _reservationRepository = reservationRepository;
            _logger = logger;
            _cartRepository = cartRepository;
            _paymentExtensions = paymentExtensions;
        }

        public async Task<Unit> Handle(HandlePaymentIntentSucceededAsyncCommand request, CancellationToken cancellationToken)
        {
            //var stripeEvent = request.stripeEvent;
            //if (stripeEvent.Data.Object is not PaymentIntent intent)
            //    return Unit.Value;

            ////  Read metadata
            //if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
            //    !intent.Metadata.TryGetValue("version", out var versionStr))
            //    return Unit.Value;

            //if (!Guid.TryParse(orderIdStr, out var orderId) ||
            //    !int.TryParse(versionStr, out var version))
            //    return Unit.Value;

            //// Load order
            //var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            //if (order == null) return Unit.Value;

            //// HARD SECURITY CHECKS 
            //if (order.PaymentIntentId != intent.Id) return Unit.Value;
            //if (order.PaymentVersion != version) return Unit.Value;
            //var paidAmount = intent.Amount / 100m;
            //if (decimal.Round(order.TotalPrice, 2) != decimal.Round(paidAmount, 2))
            //    return Unit.Value;

            //if (order.Status != OrderStatus.Pending) return Unit.Value;

            //// Mark order as paid
            //order.Status = OrderStatus.Confirmed;

            //var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            //if (payment == null) return Unit.Value;

            //payment.Status = PaymentStatus.Completed;


            //foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            //{
            //    try
            //    {
            //        await _inventoryRepository.FinalizeStockDeductionAsync(
            //            item.InvetoryId,
            //            item.Quantity,
            //            order is FromStoreOrder
            //        );
            //        await _reservationRepository.UpdateReservationStatusAsync(
            //            (Guid)item.ReservationId,
            //            ReservationStatus.Completed);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex,
            //            "Inventory finalization failed. Order {OrderId}, Inventory {InventoryId}",
            //            order.Id, item.InvetoryId);
            //    }
            //}

            //await _paymentExtensions.IncrementClientOrdersAsync(order.ClientId);
            //await _orderRepository.UpdateAsync(order);
            //await _paymentRepository.UpdateAsync(payment);
            //////  Clear cart
            //var cart = await _cartRepository.GetActiveCartAsync(order.ClientId);
            //await _cartRepository.DeleteAsync(cart);
            //await _cartRepository.CreateCartAsync(order.ClientId);

            //_paymentExtensions.PublishPaymentEvent(order, "success", "Payment completed successfully.");
            //var client = await _clientRepository.GetByIdAsync(order.ClientId);
            //if (order.OrderType == OrderType.Online)
            //{
            //    await _paymentExtensions.SendOrderConfirmationEmailAsync(order, client);
            //}
            //else
            //{
            //    var pickupCode = RandomNumberGenerator
            //                        .GetInt32(0, 1_000_000)
            //                        .ToString("D7");

            //    await _orderRepository.UpdatePickupCodeHashAsync(
            //        order.Id,
            //        _paymentExtensions.ComputeSha256(pickupCode));
            //    await _paymentExtensions.SendPickupEmailAsync(order, client, pickupCode, ((FromStoreOrder)order).StoreId);
            //}
            return Unit.Value;
        }
    }
}
