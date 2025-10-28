using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface ICategoryService
    {
        Task<Response<CategoryResponseDto>> GetCategoryByIdAsync(Guid Id);
        Task<Response<IEnumerable<CategoryResponseDto>>> SearchCategoriesByNameAsync(string name);
        Task<Response<IEnumerable<CategoryResponseDto>>> GetAllCategorysAsync();
        Task<Response<PaginatedResult<CategoryResponseDto>>> GetAllCategoriesPaginatedAsync(int pageNumber, int pageSize);
        Task<Response<IEnumerable<CategoryResponseForAdminDto>>> GetAllCategorysForAdminAsync();
        Task<Response<CategoryResponseForAdminDto>> CreateCategoryAsync(CreateCategoryRequestDto CategoryDto);
        Task<Response<CategoryResponseDto>> UpdateCategoryAsync(Guid Id ,UpdateCategoryRequest CategoryDto);
        Task<Response<string>> ChangeCategoryLogoAsync(Guid Id ,ChangeCategoryLogoRequestDto dto);
        Task<Response<bool>> DeleteCategoryAsync(Guid Id);

    }
}
