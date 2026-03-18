using MediatR;
using SmartCare.Application.Features.Store.Commands.Delete;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Store.Commands.Restore
{
    internal class RestoreStoreCommandHandler : IRequestHandler<RestoreStoreCommand, Response<bool>>
    {
        #region Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IResponseHandler _responseHandler;
        private readonly string tag = CacheConstants.Stories;
        #endregion

        public RestoreStoreCommandHandler(
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _responseHandler = responseHandler;
        }

        public async Task<Response<bool>> Handle(RestoreStoreCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var store = await _unitOfWork.Stores.GetByIdAsync(Id,true);
            if (store == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            store.Restore();

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache for store
            await _redisCacheService.DeleteKeysByTag(tag);

            return _responseHandler.Success(true);
        }
    }
}
