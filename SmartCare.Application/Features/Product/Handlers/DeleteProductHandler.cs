using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public DeleteProductHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IImageUploaderService imageUploaderService,
            IRedisCacheService redisCacheService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var productId = request.productId;

            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            await _unitOfWork.Products.DeleteAsync(product);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _redisCacheService.DeleteKeysByTag(tag);
 
            return  _responseHandler.Success(true, SystemMessages.RECORD_DELETED) ;
        }
    }
}