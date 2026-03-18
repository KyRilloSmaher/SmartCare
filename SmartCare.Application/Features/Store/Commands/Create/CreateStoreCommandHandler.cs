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

namespace SmartCare.Application.Features.Store.Commands.Create
{
    public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Response<StoreResponseForAdminDto>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public CreateStoreCommandHandler(
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService)
        {
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
        }

        public async Task<Response<StoreResponseForAdminDto>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
        {
            var StoreDto = request.StoreDto;
            var store = _mapper.Map<Domain.Entities.Store>(StoreDto);

            await _unitOfWork.Stores.AddAsync(store);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);
            _backgroundJobService.Enqueue(()=>_unitOfWork.Inventories.CreateInventoryRecordsForBranchBulkAsync(store.Id));
            var createdStoreDto = _mapper.Map<StoreResponseForAdminDto>(store);
            return _responseHandler.Success(createdStoreDto);
        }
    }
}