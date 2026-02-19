using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Commands;
using SmartCare.Application.DTOs.Product.Responses;
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
    public class UpdateProductHandler : IRequestHandler<UpdateProductAsyncCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public UpdateProductHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(UpdateProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var ProductDto = request.ProductDto;
            var Id = request.Id;
            if (Id == Guid.Empty || ProductDto == null)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>(SystemMessages.INVALID_INPUT);

            var product = await _productRepository.GetByIdAsync(Id, true);
            if (product == null)
                return _responseHandler.NotFound<ProductResponseDtoForAdmin>(SystemMessages.NOT_FOUND);

            _mapper.Map(ProductDto, product);
            var updatedProduct = await _productRepository.UpdateAsync(product);


            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedProductDto = _mapper.Map<ProductResponseDtoForAdmin>(updatedProduct);
            return _responseHandler.Success(updatedProductDto, SystemMessages.RECORD_UPDATED);
        }
    }
}
