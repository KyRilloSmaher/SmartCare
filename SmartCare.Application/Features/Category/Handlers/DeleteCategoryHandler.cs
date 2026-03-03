using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public DeleteCategoryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteCategoryAsyncCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
            if (category == null)
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);

            await _unitOfWork.Categories.DeleteAsync(category);
            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _redisCacheService.DeleteKeysByTag(tag);
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }
    }
}