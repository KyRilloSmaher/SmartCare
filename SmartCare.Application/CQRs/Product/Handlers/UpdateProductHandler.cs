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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Product.Handlers
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductAsyncCommand, Response<ProductResponseDtoForAdmin>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public UpdateProductHandler(
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

        public async Task<Response<ProductResponseDtoForAdmin>> Handle(UpdateProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var ProductDto = request.ProductDto;
            var Id = request.Id;

            if (Id == Guid.Empty || ProductDto == null)
                return _responseHandler.BadRequest<ProductResponseDtoForAdmin>(SystemMessages.INVALID_INPUT);

            var product = await _unitOfWork.Products.GetByIdAsync(Id, true);
            if (product == null)
                return _responseHandler.NotFound<ProductResponseDtoForAdmin>(SystemMessages.NOT_FOUND);

            _mapper.Map(ProductDto, product);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedProductDto = _mapper.Map<ProductResponseDtoForAdmin>(product);
            return _responseHandler.Success(updatedProductDto, SystemMessages.RECORD_UPDATED);
        }
    }
}