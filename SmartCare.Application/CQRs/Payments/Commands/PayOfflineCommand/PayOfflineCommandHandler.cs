
using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.CQRs.Payments.Commands.PayOfflineCommand
{
    public class PayOfflineCommandHandler : IRequestHandler<PayOfflineCommand, Response<PaymentResponseDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly PaymentExtensions _paymentExtensions;
        private readonly IMapper _mapper;

        public PayOfflineCommandHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobs,
            PaymentExtensions paymentExtensions,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _backgroundJobs = backgroundJobs;
            _paymentExtensions = paymentExtensions;
            _mapper = mapper;
        }

        public async Task<Response<PaymentResponseDTO>> Handle(PayOfflineCommand request, CancellationToken cancellationToken)
        {
            var orderCode = request.orderCode;
            var hashedCode = _paymentExtensions.ComputeSha256(orderCode);

            // 1. Get the order by pickup code
            var order = await _unitOfWork.Orders.GetOrderByPickUpCode(hashedCode);
            if (order is null)
                return _responseHandler.BadRequest<PaymentResponseDTO>(SystemMessages.ORDER_NOT_FOUND);

            // 2. Only pending orders can be paid offline
            if (order.Status != OrderStatus.Pending)
                return _responseHandler.BadRequest<PaymentResponseDTO>("Order is not payable.");

            // 3. Check if a payment already exists
            var existingPayment = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(order.Id);

            if (existingPayment is null)
            {
                // Create new offline payment
                var payment = new Domain.Entities.Payment(order.Id, order.TotalPrice ,PaymentMethod.Cash ,null ,null);
                await _unitOfWork.Payments.AddAsync(payment);
                order.PaymenId = payment.Id;

            }

            // 4. Update order
            order.Status = OrderStatus.Completed;
            existingPayment.MarkCompleted();

            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Finalize inventory and reservations (background job)
            _backgroundJobs.Enqueue(() => _paymentExtensions.FinishReservationsAsync(order.Id));

            // 6. Increment client stats & publish event
            await _paymentExtensions.IncrementClientOrdersAsync(order.ClientId);
            _paymentExtensions.PublishPaymentEvent(order, "success", "Offline payment completed successfully.");
            var response = _mapper.Map<PaymentResponseDTO>(existingPayment);
            return _responseHandler.Success(response);
        }
    }
}