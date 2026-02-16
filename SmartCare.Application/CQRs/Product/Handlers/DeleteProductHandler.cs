using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Commands;
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

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public DeleteProductHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            var result = await _productRepository.DeleteAsync(product);

            if (result)
            {
                await _redisCacheService.DeleteKeysByTag(tag);
            }

            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
