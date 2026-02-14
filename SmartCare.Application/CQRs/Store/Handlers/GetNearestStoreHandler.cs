using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Store.Queries;
using SmartCare.Application.DTOs.Stores.Responses;
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

namespace SmartCare.Application.CQRs.Store.Handlers
{
    public class GetNearestStoreHandler : IRequestHandler<GetNearestStoreAsyncQuery , Response<StoreResponseDto>>
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;

        #endregion
        public GetNearestStoreHandler(IStoreRepository storeRepository, IRedisCacheService redisCacheService, IMapper mapper, IMapService mapService, IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<StoreResponseDto>> Handle(GetNearestStoreAsyncQuery request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var stores = await _storeRepository.GetAllStoresAsync();

            SmartCare.Domain.Entities.Store? nearestStore = null;
            float minDistance = float.MaxValue;

            foreach (var store in stores)
            {
                var dist = _mapService.CalculateDistanceKm(dto.Latitude, dto.Longitude,
                                                             store.Latitude, store.Longitude);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestStore = store;
                }
            }
            if (nearestStore == null)
                return _responseHandler.NotFound<StoreResponseDto>(SystemMessages.NOT_FOUND);
            var nearestStoreDto = _mapper.Map<StoreResponseDto>(nearestStore);

            return _responseHandler.Success(nearestStoreDto);
        }
    }
}
