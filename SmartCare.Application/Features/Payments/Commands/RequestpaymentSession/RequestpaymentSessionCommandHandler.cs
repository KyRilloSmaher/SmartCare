
using MediatR;
using Microsoft.Extensions.Configuration;
using SmartCare.Application.CQRs.Payments.Commands.RequestpaymentSession;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Payments.Commands.RequestpaymentSession
{
    public class RequestpaymentSessionCommandHandler : IRequestHandler<RequestpaymentSessionCommand, Response<PaymentSessionResult>>
    {
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly int TimeUntilPaymentExpired;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IBackgroundJobService _backgroundJobService;
        public RequestpaymentSessionCommandHandler(
            IConfiguration configuration,
            IPaymentGatewayFactory paymentGatewayFactory,
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            IEventPublisherService eventPublisherService,
            IBackgroundJobService backgroundJobService)
        {
            _paymentGatewayFactory = paymentGatewayFactory;
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            TimeUntilPaymentExpired = configuration.GetValue<int>("ReservationTimes:MinutesForPayment");
            _eventPublisherService = eventPublisherService;
            _backgroundJobService = backgroundJobService;
        }

        public async Task<Response<PaymentSessionResult>> Handle(RequestpaymentSessionCommand request, CancellationToken cancellationToken)
        {
            IPaymentGetway _paymentGateway = _paymentGatewayFactory.Resolve(request.Provider); 
            var orderId = request.orderId;
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId,true);

            if (order == null)
                return _responseHandler.BadRequest<PaymentSessionResult>("Order not found");

            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<PaymentSessionResult>("Order not payable");

            var existingpaymentForOrder = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(orderId,true);
            PaymentSessionResult PaymentSessionResult  = null;
            if (existingpaymentForOrder is null)
            {
                CreatePaymentSessionCommand commend = new CreatePaymentSessionCommand()
                {
                    OrderId = orderId,
                    Amount = order.TotalPrice,
                    ClientId = order.ClientId,
                    Provider = request.Provider,
                };
                PaymentSessionResult = await _paymentGateway.CreateSessionAsync(commend);
                var PaymentRecord = new SmartCare.Domain.Entities.Payment(
                   orderId: orderId,
                   amount: order.TotalPrice,
                   provider: PaymentSessionResult.Provider,
                   providerReferenceId: PaymentSessionResult.ProviderReferenceId,
                   clientPaymentToken: PaymentSessionResult.ClientPaymentToken);
                await _unitOfWork.Payments.AddAsync(PaymentRecord);
                order.PaymenId = PaymentRecord.Id;
                
            }
            else
            {
                await _paymentGateway.CancelSessionAsync(existingpaymentForOrder.ProviderReferenceId);
                CreatePaymentSessionCommand commend = new CreatePaymentSessionCommand()
                {
                    OrderId = orderId,
                    Amount = order.TotalPrice,
                    ClientId = order.ClientId,
                    Provider = request.Provider,
                };
                PaymentSessionResult = await _paymentGateway.CreateSessionAsync(commend);
                existingpaymentForOrder.UpdatePaymentData(
                     order.TotalPrice,
                     PaymentSessionResult.ProviderReferenceId,
                     PaymentSessionResult.ClientPaymentToken
                );
            }

            // Update Reservation Status By Extend Reserved Time And Shcuduled new Order Cleanup

            _backgroundJobService.Enqueue(()=>ExtendOrderReservedTimeForPayment(order.Id));

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _responseHandler.Success<PaymentSessionResult>(PaymentSessionResult, "Payment Session Created");
        }
        public async Task ExtendOrderReservedTimeForPayment(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);
            foreach (var item in order.Items)
            {
                var reservation = await _unitOfWork.Reservations.GetByIdAsync((Guid)item.ReservationId, true);
                if (reservation.ExpiredAt < DateTime.UtcNow)
                {
                    reservation.ExpiredAt = DateTime.UtcNow.AddMinutes(TimeUntilPaymentExpired);
                }
            }
           await _unitOfWork.SaveChangesAsync();
            _backgroundJobService.Schedule(() => RealseOrder(order.Id), TimeSpan.FromMinutes(TimeUntilPaymentExpired));
        }

        public async Task RealseOrder(Guid orderId)
        {
 
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

            if (order is null)
                return;

            // Idempotency: don't re-expire an already finalized order
            if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled or OrderStatus.Completed)
                return;

            if (order.Items is null || !order.Items.Any())
                return;
            var reservationStatus = ReservationStatus.PaymentTimeOut;
            // Realse All Items Reservations
            foreach (var item in order.Items)
            {
                if (!item.ReservationId.HasValue)
                    continue;

                await _unitOfWork.Reservations.CancelReservationAsync(
                   reservationId: item.ReservationId.Value,
                   inventoryId: item.InvetoryId,
                   status: reservationStatus
               );
            }

            order.ChangeStatus(OrderStatus.Expired);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync();
            // Push Notifaction to User
            await _eventPublisherService.PublishOrderExpirationNotification(order.ClientId, orderId);
        }

    }
}