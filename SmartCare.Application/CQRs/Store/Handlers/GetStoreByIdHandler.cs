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
    public class GetStoreByIdHandler : IRequestHandler<GetStoreByIdAsyncQuery, Response<StoreResponseDto>>
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;


        #endregion

        public GetStoreByIdHandler(IStoreRepository storeRepository, IRedisCacheService redisCacheService, IMapper mapper, IMapService mapService, IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }


        public async Task<Response<StoreResponseDto>> Handle(GetStoreByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<StoreResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"store_{Id}";

            try
            {
                var cachedStore = await _redisCacheService.GetDataAsync<StoreResponseDto>(cacheKey, tag);
                if (cachedStore != null) return _responseHandler.Success(cachedStore);
            }
            catch (Exception) { }

            var store = await _storeRepository.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseDto>(SystemMessages.NOT_FOUND);

            var storeDto = _mapper.Map<StoreResponseDto>(store);

            await _redisCacheService.SetDataAsync(cacheKey, storeDto, tag, Time.Default);
            return _responseHandler.Success(storeDto);
        }
    }
}
