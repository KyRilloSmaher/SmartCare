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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class HandlePaymentIntentSucceededHandler : IRequestHandler<HandlePaymentIntentSucceededAsyncCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandlePaymentIntentSucceededHandler> _logger;
        private readonly PaymentExtensions _paymentExtensions;

        public HandlePaymentIntentSucceededHandler(
            IUnitOfWork unitOfWork,
            ILogger<HandlePaymentIntentSucceededHandler> logger,
            PaymentExtensions paymentExtensions)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paymentExtensions = paymentExtensions;
        }

        public async Task<Unit> Handle(HandlePaymentIntentSucceededAsyncCommand request, CancellationToken cancellationToken)
        {
            var stripeEvent = request.stripeEvent;
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return Unit.Value;

            // Read metadata
            if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                !intent.Metadata.TryGetValue("version", out var versionStr))
                return Unit.Value;

            if (!Guid.TryParse(orderIdStr, out var orderId) ||
                !int.TryParse(versionStr, out var version))
                return Unit.Value;

            // Load order
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null) return Unit.Value;

            // HARD SECURITY CHECKS 
            if (order.PaymentIntentId != intent.Id) return Unit.Value;
            if (order.PaymentVersion != version) return Unit.Value;

            var paidAmount = intent.Amount / 100m;
            if (decimal.Round(order.TotalPrice, 2) != decimal.Round(paidAmount, 2))
                return Unit.Value;

            if (order.Status != OrderStatus.Pending) return Unit.Value;

            // Mark order as paid
            order.Status = OrderStatus.Confirmed;

            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
            if (payment == null) return Unit.Value;

            payment.Status = PaymentStatus.Completed;

            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                try
                {
                    await _unitOfWork.Inventories.FinalizeStockDeductionAsync(
                        item.InvetoryId,
                        item.Quantity,
                        order is FromStoreOrder
                    );
                    await _unitOfWork.Reservations.UpdateReservationStatusAsync(
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

            await _paymentExtensions.IncrementClientOrdersAsync(order.ClientId);
            // Clear cart
            var cart = await _unitOfWork.Carts.GetActiveCartAsync(order.ClientId);
            if (cart != null)
            {
                await _unitOfWork.Carts.DeleteAsync(cart);
                await _unitOfWork.Carts.CreateCartAsync(order.ClientId);
            }

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _paymentExtensions.PublishPaymentEvent(order, "success", "Payment completed successfully.");

            var client = await _unitOfWork.UserManager.FindByIdAsync(order.ClientId);
            if (order.OrderType == OrderType.Online)
            {
                await _paymentExtensions.SendOrderConfirmationEmailAsync(order, client);
            }
            else
            {
                var pickupCode = RandomNumberGenerator
                                    .GetInt32(0, 1_000_000)
                                    .ToString("D7");

                await _unitOfWork.Orders.UpdatePickupCodeHashAsync(
                    order.Id,
                    _paymentExtensions.ComputeSha256(pickupCode));
                await _paymentExtensions.SendPickupEmailAsync(order, client, pickupCode, ((FromStoreOrder)order).StoreId);
            }

            return Unit.Value;
        }
    }
}