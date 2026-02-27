
using MediatR;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.CQRs.Payments.Commands.RequestpaymentSession;

namespace SmartCare.Application.Features.Payments.Commands.RequestpaymentSession
{
    public class CreateOrUpdatePaymentHandler : IRequestHandler<RequestpaymentSessionCommand, Response<PaymentSessionResult>>
    {
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;

        public CreateOrUpdatePaymentHandler(
            IPaymentGatewayFactory paymentGatewayFactory,
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler)
        {
            _paymentGatewayFactory = paymentGatewayFactory;
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
        }

        public async Task<Response<PaymentSessionResult>> Handle(RequestpaymentSessionCommand request, CancellationToken cancellationToken)
        {
            IPaymentGetway _paymentGateway = _paymentGatewayFactory.Resolve(request.Provider); 
            var orderId = request.orderId;
            var order = await _unitOfWork.Orders.GetOrderWithDetailsByIdAsync(orderId);

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


            // Save all changes atomically through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _responseHandler.Success<PaymentSessionResult>(PaymentSessionResult, "Payment Session Created");
        }
    }
}