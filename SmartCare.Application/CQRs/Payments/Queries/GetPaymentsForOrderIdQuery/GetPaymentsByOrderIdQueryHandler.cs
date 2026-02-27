using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Payments.Queries.GetPaymentsForOrderIdQuery;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;


namespace SmartCare.Application.CQRs.Payments.Queries.GetPaymentByIdQuery
{
    public class GetPaymentsByOrderIdQueryHandler : IRequestHandler<GetPaymentsByOrderIdQuery, Response<IEnumerable<PaymentResponseDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;

        public GetPaymentsByOrderIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }
        public async Task<Response<PaymentResponseDTO>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payments = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(request.Id);
            var paymentsResponse = _mapper.Map<PaymentResponseDTO>(payments);
            return  _responseHandler.Success(paymentsResponse);
        }

        public async Task<Response<IEnumerable<PaymentResponseDTO>>> Handle(GetPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var payments = await _unitOfWork.Payments.GetPendingPaymentByOrderIdAsync(request.orderId);
            var paymentsResponse = _mapper.Map<IEnumerable<PaymentResponseDTO>>(payments);
            return _responseHandler.Success(paymentsResponse);
        }
    }
}