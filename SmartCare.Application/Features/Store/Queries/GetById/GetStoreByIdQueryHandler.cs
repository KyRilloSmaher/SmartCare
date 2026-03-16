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

namespace SmartCare.Application.Features.Store.Queries.GetById
{
    public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Response<StoreResponseDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public GetStoreByIdQueryHandler(
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

        public async Task<Response<StoreResponseDto>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
        {
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<StoreResponseDto>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"store_{Id}";

            try
            {
                var cachedStore = await _redisCacheService.GetDataAsync<StoreResponseDto>(cacheKey, tag);
                if (cachedStore != null)
                    return _responseHandler.Success(cachedStore);
            }
            catch (Exception) { }

            var store = await _unitOfWork.Stores.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseDto>(SystemMessages.NOT_FOUND);

            var storeDto = _mapper.Map<StoreResponseDto>(store);

            await _redisCacheService.SetDataAsync(cacheKey, storeDto, tag, Time.Default);
            return _responseHandler.Success(storeDto);
        }
    }
}