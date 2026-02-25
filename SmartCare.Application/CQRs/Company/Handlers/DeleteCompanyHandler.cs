using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Companies;
        #endregion

        public DeleteCompanyHandler(
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

        public async Task<Response<bool>> Handle(DeleteCompanyAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var Company = await _unitOfWork.Companies.GetByIdAsync(Id);
            if (Company == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            await _unitOfWork.Companies.DeleteAsync(Company);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.DeleteKeysByTag(tag);

            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }
    }
}