using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Features.Orders.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.Handlers
{
    public class AcceptDeliveryHandler : IRequestHandler<AcceptDeliveryCommand, Response<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<AcceptDeliveryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;

        public AcceptDeliveryHandler(IOrderRepository orderRepository, ILogger<AcceptDeliveryHandler> logger, IUnitOfWork unitOfWork, IResponseHandler responseHandler)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
        }

        public async Task<Response<bool>> Handle(AcceptDeliveryCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == Guid.Empty)
                return _responseHandler.BadRequest<bool>("Order ID is invalid.");

            if (string.IsNullOrWhiteSpace(request.DeliveryPersonId))
                return _responseHandler.BadRequest<bool>("Delivery person ID is invalid.");

            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);

            if (order is null)
                return _responseHandler.NotFound<bool>("Order not found.");

            // Check if order is ready for delivery acceptance
            if (order.Status != OrderStatus.Ready_To_Ship)
                return _responseHandler.BadRequest<bool>(
                    $"Cannot accept delivery. Current status: {order.Status}");

            // Update order status to DELIVERY_ACCEPTED
            order.Status = OrderStatus.DELIVERY_ACCEPTED;
            order.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return _responseHandler.Success(true, "Delivery accepted successfully.");
        }
    }
}
