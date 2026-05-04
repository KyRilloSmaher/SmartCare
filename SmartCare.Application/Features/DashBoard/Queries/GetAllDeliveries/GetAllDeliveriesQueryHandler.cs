using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Delivery;
using SmartCare.Application.Features.DashBoard.Queries.GetAllAdmins;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllDeliveries
{
    public class GetAllDeliveriesQueryHandler : IRequestHandler<GetAllDeliveriesQuery, Response<List<DeliveryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllDeliveriesQueryHandler> _logger;
        private readonly IResponseHandler _responseHandler;
        public GetAllDeliveriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllDeliveriesQueryHandler> logger, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _responseHandler = responseHandler;
        }

        public async Task<Response<List<DeliveryDto>>> Handle(GetAllDeliveriesQuery request, CancellationToken cancellationToken)
        {
            var deliveries = await _unitOfWork.UserManager.GetUsersInRoleAsync("DELIVERY");
            if (deliveries == null || !deliveries.Any())
            {
                _logger.LogWarning("No deliveries found in the system.");
                return _responseHandler.NotFound<List<DeliveryDto>>("No deliveries found.");
            }
            var deliveryDtos = _mapper.Map<List<DeliveryDto>>(deliveries);
            return _responseHandler.Success(deliveryDtos, "Deliveries retrieved successfully.");

        }
    }
}
