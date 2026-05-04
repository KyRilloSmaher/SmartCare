using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllNonConfirmedPharmacist
{
    public class GetAllNonConfirmedPharmacistQueryHandler : IRequestHandler<GetAllNonConfirmedPharmacistQuery, Response<List<PharmacistProfileDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllNonConfirmedPharmacistQueryHandler> _logger;

        public GetAllNonConfirmedPharmacistQueryHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, IMapper mapper, ILogger<GetAllNonConfirmedPharmacistQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<List<PharmacistProfileDto>>> Handle(GetAllNonConfirmedPharmacistQuery request, CancellationToken cancellationToken)
        {
            var pharmacists = await _unitOfWork.Pharmacists.GetUnconfirmedPharmacistsAsync();
            if (pharmacists == null || !pharmacists.Any())
            {
                _logger.LogInformation("No non-confirmed pharmacists found.");
                return _responseHandler.NotFound<List<PharmacistProfileDto>>("No non-confirmed pharmacists found.");
            }

            var pharmacistDtos = _mapper.Map<List<PharmacistProfileDto>>(pharmacists);
            _logger.LogInformation("Successfully retrieved {Count} non-confirmed pharmacists.", pharmacistDtos.Count);
            return _responseHandler.Success(pharmacistDtos);
        }
    }
}
