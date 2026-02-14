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
    public class SearchStoresByNameHandler : IRequestHandler<SearchStoresByNameAsyncQuery, Response<IEnumerable<StoreResponseDto>>>
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;


        #endregion

        public SearchStoresByNameHandler(IStoreRepository storeRepository, IRedisCacheService redisCacheService, IMapper mapper, IMapService mapService, IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<IEnumerable<StoreResponseDto>>> Handle(SearchStoresByNameAsyncQuery request, CancellationToken cancellationToken)
        {
            var name = request.name;
            if (string.IsNullOrWhiteSpace(name))
                return _responseHandler.BadRequest<IEnumerable<StoreResponseDto>>(SystemMessages.INVALID_INPUT);
            var stores = await _storeRepository.SearchStoresAsync(name);
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseDto>>(stores);
            return _responseHandler.Success(storeDtos);
        }
    }
}
