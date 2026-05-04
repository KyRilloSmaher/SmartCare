using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.CQRs.Payments.Commands.HandlePaymentSucceededCommand;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.CQRs.Payments.Commands.HandlePaymentFailedCommand
{
    public class HandlePaymentIntentFailedCommandHandler : IRequestHandler<HandlePaymentIntentFailedAsyncCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandlePaymentIntentFailedCommandHandler> _logger;
        private readonly PaymentExtensions _paymentExtensions;
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;

        public HandlePaymentIntentFailedCommandHandler(IUnitOfWork unitOfWork, ILogger<HandlePaymentIntentFailedCommandHandler> logger, PaymentExtensions paymentExtensions, IResponseHandler responseHandler, IBackgroundJobService backgroundJobService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paymentExtensions = paymentExtensions;
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
        }

        public async Task<Response<bool>> Handle(HandlePaymentIntentFailedAsyncCommand request, CancellationToken cancellationToken)
        {
            Guid OrderId = (Guid)request.paymentwebHookEventResult.OrderId;
            var paymentResult = request.paymentwebHookEventResult;
            // Load order
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(OrderId);

            if (order is null)
            {
                _logger.LogError("Handle payment Failed Method : Trying To Fetch Order With Null Id");
                return _responseHandler.Failed<bool>("Failed To handle payment Failling");
            }
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogError("Handle payment Failed Method : Order Status Is not Pending");
                return _responseHandler.Failed<bool>("Failed To handle payment Failed Because its Already Not In Pending Status");
            }
            var existingPayment = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(OrderId,true);
            if (existingPayment is null) {
                _logger.LogError($"No Payment Found By OrderId {order.Id}!");
            }
            existingPayment.MarkFailed();
            order.ChangeStatus(OrderStatus.PaymentFailed);
            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _paymentExtensions.PublishPaymentEvent(order, "failed", "Payment failed");
            return _responseHandler.Success(false, "Payment Marked As Failed");
        }
    }
}