using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Category.Commands;
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

namespace SmartCare.Application.CQRs.Category.Handlers
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryAsyncCommand, Response<bool>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Categories;


        #endregion
        public DeleteCategoryHandler(
            IResponseHandler responseHandler,
            ICategoryRepository categoryRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _categoryRepository = categoryRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        public async Task<Response<bool>> Handle(DeleteCategoryAsyncCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            var result = await _categoryRepository.DeleteAsync(category);
            if (result)
            {
                await _redisCacheService.DeleteKeysByTag(tag);
            }
            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
