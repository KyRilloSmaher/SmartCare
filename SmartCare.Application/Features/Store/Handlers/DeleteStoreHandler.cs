using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Store.Commands;
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
    public class DeleteStoreHandler : IRequestHandler<DeleteStoreAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public DeleteStoreHandler(
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

        public async Task<Response<bool>> Handle(DeleteStoreAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var store = await _unitOfWork.Stores.GetByIdAsync(Id);
            if (store == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            store.IsDeleted = true;

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);

            return _responseHandler.Success(true);
        }
    }
}