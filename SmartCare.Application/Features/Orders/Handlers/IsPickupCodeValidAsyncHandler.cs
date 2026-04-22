using MediatR;
using SmartCare.Application.CQRs.Order.Extension;
using SmartCare.Application.Features.Orders.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Orders.Handlers
{
    public class IsPickupCodeValidAsyncHandler : IRequestHandler<IsPickupCodeValidAsyncQuery, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public IsPickupCodeValidAsyncHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(IsPickupCodeValidAsyncQuery request, CancellationToken cancellationToken)
        {
            var pickupOrder = await _unitOfWork.Orders.GetOfflineOrderAsync(request.OrderId);
            if (pickupOrder == null)
                return _responseHandler.NotFound<bool>(SystemMessages.ORDER_NOT_FOUND);

            var hashedCode = OrderExtensions.ComputeSha256(request.verifyCode);

            // 3. Compare hashes
            var isValid = string.Equals(
                pickupOrder.PickupCodeHash,
                hashedCode,
                StringComparison.OrdinalIgnoreCase
            );

            if (!isValid)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_PICKUP_CODE);

            return _responseHandler.Success(true, SystemMessages.PICKUP_CODE_VALID);
        }
    }
}