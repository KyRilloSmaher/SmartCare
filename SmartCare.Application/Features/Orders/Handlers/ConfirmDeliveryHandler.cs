using MediatR;
using SmartCare.Application.Features.Orders.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Handlers
{
    public class ConfirmDeliveryHandler
        : IRequestHandler<ConfirmDeliveryCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;

        public ConfirmDeliveryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
        }

        public async Task<Response<bool>> Handle(
            ConfirmDeliveryCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>("Order ID is invalid.");

            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);

            if (order is null)
                return _responseHandler.NotFound<bool>("Order not found.");

            if (order.Status != OrderStatus.Shipped)
                return _responseHandler.BadRequest<bool>(
                    $"Order cannot be confirmed. Current status: {order.Status}");

            order.ChangeStatus(OrderStatus.Completed);
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return _responseHandler.Success(true, "Order delivered successfully.");
        }
    }
}
