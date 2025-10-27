using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;


namespace SmartCare.Application.Services
{
    public class StoreService : IStoreService
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        #endregion
        #region Constructor
        public StoreService(IStoreRepository storeRepository, IMapper mapper, IResponseHandler responseHandler, IMapService mapService)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
            _responseHandler = responseHandler;
            _mapService = mapService;
        }
        #endregion
        #region Methods
        public async Task<Response<StoreResponseDto>> GetStoreByIdAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<StoreResponseDto>(SystemMessages.INVALID_INPUT);
            var store = await _storeRepository.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseDto>(SystemMessages.NOT_FOUND);
            var storeDto = _mapper.Map<StoreResponseDto>(store);
            return _responseHandler.Success(storeDto);
        }

        public async Task<Response<IEnumerable<StoreResponseDto>>> SearchStoresByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return _responseHandler.BadRequest<IEnumerable<StoreResponseDto>>(SystemMessages.INVALID_INPUT);
            var stores = await _storeRepository.SearchStoresAsync(name);
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseDto>>(stores);
            return _responseHandler.Success(storeDtos);
        }

        public async Task<Response<IEnumerable<StoreResponseDto>>> GetAllStoresAsync()
        {
            var stores = await _storeRepository.GetAllAsync();
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseDto>>(stores);
            return _responseHandler.Success(storeDtos);
        }

        public async Task<Response<IEnumerable<StoreResponseForAdminDto>>> GetAllStoresForAdminAsync()
        {
            var stores = await _storeRepository.GetAllAsync();
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseForAdminDto>>(stores);
            return _responseHandler.Success(storeDtos);
        }

        public async  Task<Response<StoreResponseForAdminDto>> CreateStoreAsync(CreateStoreRequestDto StoreDto)
        {
            var store = _mapper.Map<Store>(StoreDto);
            await _storeRepository.AddAsync(store);
            var createdStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(createdStoreDto);

        }

        public async Task<Response<StoreResponseForAdminDto>> UpdateStoreAsync(Guid Id, UpdateStoreRequestDto StoreDto)
        {
            if (Id == Guid.Empty || StoreDto == null)
                return _responseHandler.BadRequest<StoreResponseForAdminDto>(SystemMessages.INVALID_INPUT);
            var store = await _storeRepository.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseForAdminDto>(SystemMessages.NOT_FOUND);
            _mapper.Map(StoreDto, store);
            await _storeRepository.UpdateAsync(store);
            var updatedStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(updatedStoreDto);

        }

        public async Task<Response<bool>> DeleteStoreAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var store = await _storeRepository.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            store.IsDeleted = true;
            await _storeRepository.UpdateAsync(store);
            return _responseHandler.Success(true);

        }
        public async Task<Response<StoreResponseDto>> GetNearestStoreAsync(AddressValuesDto dto)
        {

            var stores = await _storeRepository.GetAllStoresAsync();

            Store? nearestStore = null;
            float minDistance = float.MaxValue;

            foreach (var store in stores)
            {
                var dist = _mapService.CalculateDistanceKm(dto.Latitude,dto.Longitude,
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

            return  _responseHandler.Success(nearestStoreDto); ;
        }
        #endregion
    }
}
