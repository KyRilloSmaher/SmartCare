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
using static Azure.Core.HttpHeader;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class GetDetailsOfProductByNameHandler : IRequestHandler<GetDetailsOfProductByNameQuery, Response<ProductResponseDtoForClient>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IProductRepository _productRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Products;


        #endregion

        public GetDetailsOfProductByNameHandler(IResponseHandler responseHandler, IProductRepository productRepository, IImageUploaderService imageUploaderService, IRedisCacheService redisCacheService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _productRepository = productRepository;
            _imageUploaderService = imageUploaderService;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }


        public async Task<Response<ProductResponseDtoForClient>> Handle(GetDetailsOfProductByNameQuery request, CancellationToken cancellationToken)
        {
            var NameEn = request.NameEn;
            if (string.IsNullOrWhiteSpace(NameEn))
                return _responseHandler.Failed<ProductResponseDtoForClient>("Product name must be provided.");

            string cacheKey = $"product_name_{NameEn.ToLower().Replace(" ", "_")}";

            try
            {
                var cachedProduct = await _redisCacheService.GetDataAsync<ProductResponseDtoForClient>(cacheKey, tag);
                if (cachedProduct != null) return _responseHandler.Success(cachedProduct);
            }
            catch (Exception) { /* Continue to DB */ }

            var product = await _productRepository.SearchProductByNameAsync(NameEn);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForClient>("Product not found.");

            var productDto = _mapper.Map<ProductResponseDtoForClient>(product);

            await _redisCacheService.SetDataAsync(cacheKey, productDto, tag, Time.Default);

            return _responseHandler.Success(productDto);
        }
    }
}
