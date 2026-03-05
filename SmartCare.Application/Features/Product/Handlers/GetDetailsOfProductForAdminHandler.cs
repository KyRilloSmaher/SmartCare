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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class GetDetailsOfProductForAdminHandler : IRequestHandler<GetDetailsOfProductForAdminQuery, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public GetDetailsOfProductForAdminHandler(
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

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(GetDetailsOfProductForAdminQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;

            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>(SystemMessages.INVALID_INPUT);

            string cacheKey = $"product_admin_{productId}";

            try
            {
                var cachedProduct = await _redisCacheService.GetDataAsync<ProductResponseDtoForAdmin>(cacheKey, tag);
                if (cachedProduct != null)
                    return _responseHandler.Success(cachedProduct);
            }
            catch (Exception) { /* Continue to DB */ }

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return _responseHandler.Failed<ProductResponseDtoForAdmin>(SystemMessages.NOT_FOUND);

            var productDto = _mapper.Map<ProductResponseDtoForAdmin>(product);

            await _redisCacheService.SetDataAsync(cacheKey, productDto, tag, Time.Default);

            return _responseHandler.Success(productDto);
        }
    }
}