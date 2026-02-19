using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Product.Queries;
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
    public class GetDetailsOfProductForUserHandler : IRequestHandler<GetDetailsOfProductForUserQuery, Response<ProductResponseDtoForClient>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public GetDetailsOfProductForUserHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }


        public async Task<Response<ProductResponseDtoForClient>> Handle(GetDetailsOfProductForUserQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<ProductResponseDtoForClient>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"product_details:{productId}";

            var cachedProduct = await _redisCacheService.GetDataAsync<ProductResponseDtoForClient>(cacheKey, tag);
            if (cachedProduct != null)
            {
                return _responseHandler.Success(cachedProduct);
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForClient>(SystemMessages.NOT_FOUND);

            var productDto = _mapper.Map<ProductResponseDtoForClient>(product);

            await _redisCacheService.SetDataAsync(cacheKey, productDto, tag, Time.Default);

            return _responseHandler.Success(productDto);
        }
    }
}
