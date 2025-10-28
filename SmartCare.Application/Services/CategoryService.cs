using AutoMapper;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Extentions;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;


namespace SmartCare.Application.Services
{
    public class CategoryService : ICategoryService
    {
        #region Feilds
        private readonly IResponseHandler _responseHandler;
        private readonly ICategoryRepository _categoryRepository ;
        private readonly IImageUploaderService _imageUploaderService ;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public CategoryService(IResponseHandler responseHandler, ICategoryRepository categoryRepository, IImageUploaderService imageUploaderService, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _categoryRepository = categoryRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        #endregion

        #region Methods
        public async Task<Response<CategoryResponseDto>> GetCategoryByIdAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);
            var categoryDto = _mapper.Map<CategoryResponseDto>(category);
            return _responseHandler.Success(categoryDto);
        }

        public async Task<Response<IEnumerable<CategoryResponseDto>>> GetAllCategorysAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }

        public async Task<Response<IEnumerable<CategoryResponseForAdminDto>>> GetAllCategorysForAdminAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesForAdminAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseForAdminDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }
           

        public async Task<Response<CategoryResponseDto>> UpdateCategoryAsync(Guid Id, UpdateCategoryRequest CategoryDto)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<CategoryResponseDto>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id, true);
            if (category == null)
                return _responseHandler.Failed<CategoryResponseDto>(SystemMessages.NOT_FOUND);
             _mapper.Map(CategoryDto, category);
            var updatedCategory = await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
            var updatedCategoryDto = _mapper.Map<CategoryResponseDto>(updatedCategory);
            return _responseHandler.Success(updatedCategoryDto, SystemMessages.RECORD_UPDATED);
        }

        public async Task<Response<bool>> DeleteCategoryAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id);
            if (category == null)
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            var result = await _categoryRepository.DeleteAsync(category);
            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<string>> ChangeCategoryLogoAsync(Guid Id, ChangeCategoryLogoRequestDto CategoryDto)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<string>(SystemMessages.INVALID_INPUT);
            var category = await _categoryRepository.GetByIdAsync(Id, true);
            if (category is null)
                return _responseHandler.Failed<string>(SystemMessages.NOT_FOUND);
            // Delete old image 
            var oldImageUrl = category.LogoUrl;
            var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(CategoryDto.Image, ImageFolder.CategoryImages);
            if (uploadResult.Error != null)
            {
                await _categoryRepository.RollBackAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            category.LogoUrl = uploadResult.Url.ToString();
            var updateResult = await _categoryRepository.UpdateAsync(category);
            return _responseHandler.Success(updateResult.LogoUrl);
        }

        public async Task<Response<CategoryResponseForAdminDto>> CreateCategoryAsync(CreateCategoryRequestDto CategoryDto)
        {
            string? uploadedImageUrl = null;

            try
            {
                if (CategoryDto.Logo is not null)
                {
                    var uploadResult = await _imageUploaderService.UploadImageAsync(CategoryDto.Logo, ImageFolder.CategoryImages);

                    if (uploadResult.Error != null)
                        return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FILE_UPLOAD_FAILED);

                    uploadedImageUrl = uploadResult.Url.ToString();
                }

                await _categoryRepository.BeginTransactionAsync();

                var category = _mapper.Map<Category>(CategoryDto);
                category.LogoUrl = uploadedImageUrl;

                var createResult = await _categoryRepository.AddAsync(category);
                if (createResult is null)
                {
                    await _categoryRepository.RollBackAsync();
                    return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
                }

                await _categoryRepository.CommitTransactionAsync();

                var createdCategoryDto = _mapper.Map<CategoryResponseForAdminDto>(createResult);
                return _responseHandler.Success(createdCategoryDto, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                await _categoryRepository.RollBackAsync();

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                    await _imageUploaderService.DeleteImageByUrlAsync(uploadedImageUrl);

                return _responseHandler.Failed<CategoryResponseForAdminDto>(SystemMessages.FAILED);
            }
        }


        public async Task<Response<IEnumerable<CategoryResponseDto>>> SearchCategoriesByNameAsync(string name)
        {
            var categories = await _categoryRepository.SearchCategoryByNameAsync(name);
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }

        public async Task<Response<PaginatedResult<CategoryResponseDto>>> GetAllCategoriesPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<CategoryResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);

            var query = _categoryRepository.GetAllCategoriesQuerable();

            var projectedQuery = _mapper.ProjectTo<CategoryResponseDto>(query);

            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);

            return _responseHandler.Success(paginatedResult);
        }

        #endregion
    }
}
