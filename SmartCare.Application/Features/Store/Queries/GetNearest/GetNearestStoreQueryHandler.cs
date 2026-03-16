using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Store.Queries.GetNearest
{
    public class GetNearestStoreQueryHandler : IRequestHandler<GetNearestStoreQuery, Response<StoreResponseDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public GetNearestStoreQueryHandler(
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

        public async Task<Response<StoreResponseDto>> Handle(GetNearestStoreQuery request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var stores = await _unitOfWork.Stores.GetAllStoresAsync();

            Domain.Entities.Store? nearestStore = null;
            double minDistance = double.MaxValue; // Changed to double for better precision

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