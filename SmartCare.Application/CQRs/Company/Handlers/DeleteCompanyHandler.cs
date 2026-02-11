using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Company.Commands;
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

namespace SmartCare.Application.CQRs.Company.Handlers
{
    public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyAsyncCommand, Response<bool>>
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Companies;

        #endregion
        public DeleteCompanyHandler(
            IResponseHandler responseHandler,
            ICompanyRepository companyRepository,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _CompanyRepository = companyRepository;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteCompanyAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id);
            if (Company == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            var result = await _CompanyRepository.DeleteAsync(Company);
            if (result)
            {
                await _redisCacheService.DeleteKeysByTag(tag);
            }
            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
    }
}
