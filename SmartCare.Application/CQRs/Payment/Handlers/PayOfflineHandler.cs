using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Commands;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.DTOs.payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paymentEntity = SmartCare.Domain.Entities.Payment;

namespace SmartCare.Application.CQRs.Payment.Handlers
{
    public class PayOfflineHandler : IRequestHandler<PayOfflineAsyncCommand, Response<PaymentResult>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly PaymentExtensions _paymentExtensions;

        public PayOfflineHandler(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IResponseHandler responseHandler, IBackgroundJobService backgroundJobs, PaymentExtensions paymentExtensions)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _responseHandler = responseHandler;
            _backgroundJobs = backgroundJobs;
            _paymentExtensions = paymentExtensions;
        }

        public async Task<Response<PaymentResult>> Handle(PayOfflineAsyncCommand request, CancellationToken cancellationToken)
        {
            var orderCode = request.orderCode;
            var hashedCode = _paymentExtensions.ComputeSha256(orderCode);

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
                var payment = new paymentEntity
                {
                    OrderId = order.Id,
                    Status = PaymentStatus.Completed,
                    Amount = order.TotalPrice,
                    PaymentIntentId = null,
                    CreatedAt = DateTime.UtcNow,
                    Method = Domain.Enums.PaymentMethod.Cash
                };
                await _paymentRepository.Add(payment);
                order.PaymentIntentId = payment.Id.ToString();
            }

            // 4. Update order
            order.Status = OrderStatus.Completed;
            await _orderRepository.UpdateAsync(order);

            // 5. Finalize inventory and reservations
            _backgroundJobs.Enqueue(() => _paymentExtensions.FinishReservationsAsync(order.Id));

            // 6. Increment client stats & publish event
            await _paymentExtensions.IncrementClientOrdersAsync(order.ClientId);
            _paymentExtensions.PublishPaymentEvent(order, "success", "Offline payment completed successfully.");

            return _responseHandler.Success(
                new PaymentResult(true, SystemMessages.PAYMENT_PROCESSED, hashedCode)
            );
        }
    }
}
