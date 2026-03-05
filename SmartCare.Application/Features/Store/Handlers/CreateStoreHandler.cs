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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Store.Handlers
{
    public class CreateStoreHandler : IRequestHandler<CreateStoreAsyncCommand, Response<StoreResponseForAdminDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public CreateStoreHandler(
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

        public async Task<Response<StoreResponseForAdminDto>> Handle(CreateStoreAsyncCommand request, CancellationToken cancellationToken)
        {
            var StoreDto = request.StoreDto;
            var store = _mapper.Map<SmartCare.Domain.Entities.Store>(StoreDto);

            await _unitOfWork.Stores.AddAsync(store);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);

            var createdStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(createdStoreDto);
        }
    }
}