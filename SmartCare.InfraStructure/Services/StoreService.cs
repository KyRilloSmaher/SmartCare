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


namespace SmartCare.InfraStructure.Services
{
    public class StoreService : IStoreService
    {
        #region Feilds
        private readonly IStoreRepository _storeRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        string tag = CacheConstants.Stories;

        #endregion
        #region Constructor
        public StoreService(IStoreRepository storeRepository,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IMapService mapService,
            IResponseHandler responseHandler)
        {
            _storeRepository = storeRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _mapService = mapService;
            _responseHandler = responseHandler;
        }

        #endregion
        #region Methods
        public async Task<Response<StoreResponseDto>> GetStoreByIdAsync(Guid Id)
        {
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
            string cacheKey = "stores_client_all";

            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<IEnumerable<StoreResponseDto>>(cacheKey, tag);
                if (cachedData != null) return _responseHandler.Success(cachedData);
            }
            catch (Exception) { }

            var stores = await _storeRepository.GetAllAsync();
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseDto>>(stores);

            await _redisCacheService.SetDataAsync(cacheKey, storeDtos, tag, Time.Default);
            return _responseHandler.Success(storeDtos);
        }

        public async Task<Response<IEnumerable<StoreResponseForAdminDto>>> GetAllStoresForAdminAsync()
        {
            string cacheKey = "stores_admin_all";

            try
            {
                var cachedData = await _redisCacheService.GetDataAsync<IEnumerable<StoreResponseForAdminDto>>(cacheKey, tag);
                if (cachedData != null) return _responseHandler.Success(cachedData);
            }
            catch (Exception) { /* Log error if needed */ }

            var stores = await _storeRepository.GetAllAsync();
            var storeDtos = _mapper.Map<IEnumerable<StoreResponseForAdminDto>>(stores);

            await _redisCacheService.SetDataAsync(cacheKey, storeDtos, tag, Time.Default);
            return _responseHandler.Success(storeDtos);
        }

        public async  Task<Response<StoreResponseForAdminDto>> CreateStoreAsync(CreateStoreRequestDto StoreDto)
        {
            var store = _mapper.Map<Store>(StoreDto);
            await _storeRepository.AddAsync(store);
            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
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
            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
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
            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
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

            return  _responseHandler.Success(nearestStoreDto);
        }
        #endregion
    }
}
