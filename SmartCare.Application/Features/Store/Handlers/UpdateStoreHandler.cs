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
    public class UpdateStoreHandler : IRequestHandler<UpdateStoreAsyncCommand, Response<StoreResponseForAdminDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public UpdateStoreHandler(
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

        public async Task<Response<StoreResponseForAdminDto>> Handle(UpdateStoreAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var StoreDto = request.StoreDto;

            if (Id == Guid.Empty || StoreDto == null)
                return _responseHandler.BadRequest<StoreResponseForAdminDto>(SystemMessages.INVALID_INPUT);

            var store = await _unitOfWork.Stores.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<StoreResponseForAdminDto>(SystemMessages.NOT_FOUND);

            _mapper.Map(StoreDto, store);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(updatedStoreDto);
        }
    }
}