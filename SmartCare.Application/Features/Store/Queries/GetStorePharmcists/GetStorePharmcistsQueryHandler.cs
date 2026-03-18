using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Features.Store.Queries.GetAll;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Store.Queries.GetStorePharmcists
{
    public class GetStorePharmcistsQueryHandler: IRequestHandler<GetStorePharmcistsQuery, Response<IEnumerable<PharmacistResponseDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<GetStorePharmcistsQueryHandler> _logger;

        public GetStorePharmcistsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetStorePharmcistsQueryHandler> logger, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _responseHandler = responseHandler;
        }

        public async Task<Response<IEnumerable<PharmacistResponseDto>>> Handle(GetStorePharmcistsQuery request, CancellationToken cancellationToken)
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(request.storeId);
            if (store == null) {
                return _responseHandler.BadRequest<IEnumerable<PharmacistResponseDto>>(SystemMessages.NOT_FOUND);
            }
            var pharmacists = await _unitOfWork.Stores.GetStorePharmacistsAsync(request.storeId);
            if (pharmacists == null) {
                return _responseHandler.Success<IEnumerable<PharmacistResponseDto>>(null,SystemMessages.NOT_FOUND);
            }
            var responseDtos = _mapper.Map<IEnumerable<PharmacistResponseDto>>(pharmacists);
            return  _responseHandler.Success<IEnumerable<PharmacistResponseDto>>(responseDtos, SystemMessages.NOT_FOUND);
        }
    }
}
