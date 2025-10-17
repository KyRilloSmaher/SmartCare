using AutoMapper;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{
    public class CompanyService : ICompanyService
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICompanyRepository _CompanyRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public CompanyService(IResponseHandler responseHandler, ICompanyRepository CompanyRepository, IImageUploaderService imageUploaderService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _CompanyRepository = CompanyRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        #endregion

        #region Methods
        public async Task<Response<CompanyResponseDto>> GetCompanyByIdAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CompanyResponseDto>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id);
            if (Company == null)
                return _responseHandler.NotFound<CompanyResponseDto>(SystemMessages.NOT_FOUND);
            var CompanyDto = _mapper.Map<CompanyResponseDto>(Company);
            return _responseHandler.Success(CompanyDto);
        }

        public async Task<Response<IEnumerable<CompanyResponseDto>>> GetAllCompaniesAsync()
        {
            var companies = await _CompanyRepository.GetAllCompaniesAsync();
            var companiesDto = _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);
            return _responseHandler.Success(companiesDto);
        }

        public async Task<Response<IEnumerable<CompanyResponseForAdminDto>>> GetAllCompaniesForAdminAsync()
        {
            var companies = await _CompanyRepository.GetAllCompaniesForAdminAsync();
            var companiesDto = _mapper.Map<IEnumerable<CompanyResponseForAdminDto>>(companies);
            return _responseHandler.Success(companiesDto);
        }


        public async Task<Response<CompanyResponseDto>> UpdateCompanyAsync(Guid Id, UpdateCompanyRequest CompanyDto)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CompanyResponseDto>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id, true);
            if (Company == null)
                return _responseHandler.NotFound<CompanyResponseDto>(SystemMessages.NOT_FOUND);
            _mapper.Map(CompanyDto, Company);
            var updatedCompany = await _CompanyRepository.UpdateAsync(Company);
            await _CompanyRepository.SaveChangesAsync();
            var updatedCompanyDto = _mapper.Map<CompanyResponseDto>(updatedCompany);
            return _responseHandler.Success(updatedCompanyDto, SystemMessages.RECORD_UPDATED);
        }

        public async Task<Response<bool>> DeleteCompanyAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id);
            if (Company == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            Company.IsDeleted = true;
            await _CompanyRepository.UpdateAsync(Company);
            await _CompanyRepository.SaveChangesAsync();
            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }

        public async Task<Response<string>> ChangeCompanyLogoAsync(Guid Id, ChangeCompanyLogoRequestDto CompanyDto)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);
            var Company = await _CompanyRepository.GetByIdAsync(Id, true);
            if (Company is null)
                return _responseHandler.NotFound<string>(SystemMessages.NOT_FOUND);
            // Delete old image 
            var oldImageUrl = Company.LogoUrl;
            var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(CompanyDto.Image, ImageFolder.BrandLogos);
            if (uploadResult.Error != null)
            {
                await _CompanyRepository.RollBackAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            Company.LogoUrl = uploadResult.Url.ToString();
            var updateResult = await _CompanyRepository.UpdateAsync(Company);
            return _responseHandler.Success(updateResult.LogoUrl);
        }

        public async Task<Response<CompanyResponseForAdminDto>> CreateCompanyAsync(CreateCompanyRequestDto companyDto)
        {
            string? uploadedImageUrl = null;

            try
            {
                // Upload profile image if provided
                if (companyDto.Logo is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(companyDto.Logo, ImageFolder.BrandLogos);

                    if (uploadResult.Error != null)
                        return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                await _CompanyRepository.BeginTransactionAsync();

                var company = _mapper.Map<Company>(companyDto);
                company.LogoUrl = uploadedImageUrl;

                // Add to repository
                var createdEntity = await _CompanyRepository.AddAsync(company);

                if (createdEntity is null)
                {
                    await _CompanyRepository.RollBackAsync();
                    return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
                }

                // Commit changes
                await _CompanyRepository.SaveChangesAsync();
                await _CompanyRepository.CommitTransactionAsync();

                var createdCompanyDto = _mapper.Map<CompanyResponseForAdminDto>(createdEntity);
                return _responseHandler.Success(createdCompanyDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                await _CompanyRepository.RollBackAsync();

                // Delete uploaded image if something went wrong
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CompanyResponseForAdminDto>(SystemMessages.FAILED);
            }
        }


        public async Task<Response<IEnumerable<CompanyResponseDto>>> SearchCompaniesByNameAsync(string name)
        {
            var categories = await _CompanyRepository.SearchCompaniesByNameAsync(name);
            var categoriesDto = _mapper.Map<IEnumerable<CompanyResponseDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }
        #endregion
    }
}
