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
    public class UpdateStoreHandler : IRequestHandler<UpdateStoreAsyncCommand, Response<StoreResponseForAdminDto>>
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;


        #endregion

        public UpdateStoreHandler(IStoreRepository storeRepository, IRedisCacheService redisCacheService, IMapper mapper, IMapService mapService, IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<StoreResponseForAdminDto>> Handle(UpdateStoreAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var StoreDto = request.StoreDto;
            if (Id == Guid.Empty || StoreDto == null)
                return _responseHandler.BadRequest<StoreResponseForAdminDto>(SystemMessages.INVALID_INPUT);
            var store = await _storeRepository.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseForAdminDto>(SystemMessages.NOT_FOUND);
            _mapper.Map(StoreDto, store);
            await _storeRepository.UpdateAsync(store);
            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
            var updatedStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(updatedStoreDto);
        }
    }
}
