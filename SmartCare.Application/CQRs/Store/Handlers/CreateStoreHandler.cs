using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Store.Commands;
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
    public class CreateStoreHandler : IRequestHandler<CreateStoreAsyncCommand, Response<StoreResponseForAdminDto>>
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;


        #endregion

        public CreateStoreHandler(IStoreRepository storeRepository, IRedisCacheService redisCacheService, IMapper mapper, IMapService mapService, IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<StoreResponseForAdminDto>> Handle(CreateStoreAsyncCommand request, CancellationToken cancellationToken)
        {
            var StoreDto = request.StoreDto;
            var store = _mapper.Map<SmartCare.Domain.Entities.Store>(StoreDto);
            await _storeRepository.AddAsync(store);
            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
            var createdStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(createdStoreDto);
        }
    }
}
