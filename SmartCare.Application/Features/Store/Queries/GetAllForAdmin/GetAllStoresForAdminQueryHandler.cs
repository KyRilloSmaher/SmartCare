using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Store.Queries.GetAllForAdmin
{
    public class GetAllStoresForAdminQueryHandler : IRequestHandler<GetAllStoresForAdminQuery, Response<IEnumerable<StoreResponseForAdminDto>>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public GetAllStoresForAdminQueryHandler(
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IMapService mapService,
            IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<IEnumerable<StoreResponseForAdminDto>>> Handle(GetAllStoresForAdminQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "stores_admin_all";

            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<IEnumerable<StoreResponseForAdminDto>>(cacheKey, tag);
                if (cachedData != null)
                    return _responseHandler.Success(cachedData);
            }
            catch (Exception) { /* Log error if needed */ }

            var stores = await _unitOfWork.Stores.GetAllAsync();
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseForAdminDto>>(stores);

            await _redisCacheService.SetDataAsync(cacheKey, storeDtos, tag, Time.Default);
            return _responseHandler.Success(storeDtos);
        }
    }
}