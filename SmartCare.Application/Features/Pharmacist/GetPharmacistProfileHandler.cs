using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Pharmacist
{
    public class GetPharmacistProfileHandler : IRequestHandler<GetPharmacistProfileQuery, Response<PharmacistProfileDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetPharmacistProfileHandler(IResponseHandler responseHandler, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PharmacistProfileDto>> Handle(GetPharmacistProfileQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return _responseHandler.BadRequest<PharmacistProfileDto>("User ID is invalid.");

            var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(request.UserId);

            if (pharmacist == null)
                return _responseHandler.NotFound<PharmacistProfileDto>("Pharmacist not found.");

            var pharmacistDto = _mapper.Map<PharmacistProfileDto>(pharmacist);

            return _responseHandler.Success(pharmacistDto);
        }
    }
}
