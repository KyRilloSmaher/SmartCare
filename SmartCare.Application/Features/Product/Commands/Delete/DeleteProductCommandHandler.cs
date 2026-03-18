using AutoMapper;
using MediatR;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Product.Commands.Delete
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Products;
        #endregion

        public DeleteProductCommandHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService)
         
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
        }

        public async Task<Response<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var productId = request.productId;

            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var product = await _unitOfWork.Products.GetByIdAsync(productId,true);
            if (product == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            product.Delete();

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _redisCacheService.DeleteKeysByTag(tag);
 
            return  _responseHandler.Success(true, SystemMessages.RECORD_DELETED) ;
        }
    }
}