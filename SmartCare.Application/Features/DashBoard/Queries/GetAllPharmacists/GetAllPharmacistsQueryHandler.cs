using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.Features.DashBoard.Queries.GetAllNonConfirmedPharmacist;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllPharmacists
{
    public class GetAllPharmacistsQueryHandler : IRequestHandler<GetAllPharmacistsQuery, Response<List<PharmacistProfileDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllPharmacistsQueryHandler> _logger;

        public GetAllPharmacistsQueryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, IMapper mapper, ILogger<GetAllPharmacistsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<List<PharmacistProfileDto>>> Handle(GetAllPharmacistsQuery request, CancellationToken cancellationToken)
        {
            var pharmacists = await _unitOfWork.Pharmacists.GetAllAsync();
            if (pharmacists == null || !pharmacists.Any())
            {
                _logger.LogInformation("No pharmacists found.");
                return _responseHandler.NotFound<List<PharmacistProfileDto>>("No pharmacists found.");
            }

            var pharmacistDtos = _mapper.Map<List<PharmacistProfileDto>>(pharmacists);
            _logger.LogInformation("Successfully retrieved {Count} pharmacists.", pharmacistDtos.Count);
            return _responseHandler.Success(pharmacistDtos);
        }
    }
}
