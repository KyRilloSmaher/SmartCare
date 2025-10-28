using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;


namespace SmartCare.Application.IServices
{
    public interface ICompanyService
    {
        Task<Response<CompanyResponseDto>> GetCompanyByIdAsync(Guid Id);
        Task<Response<IEnumerable<CompanyResponseDto>>> SearchCompaniesByNameAsync(string name);
        Task<Response<IEnumerable<CompanyResponseDto>>> GetAllCompaniesAsync();
        Task<Response<PaginatedResult<CompanyResponseDto>>> GetAllCompaniesPaginatedAsync(int pageNumber, int pageSize);
        Task<Response<IEnumerable<CompanyResponseForAdminDto>>> GetAllCompaniesForAdminAsync();
        Task<Response<CompanyResponseForAdminDto>> CreateCompanyAsync(CreateCompanyRequestDto CompanyDto);
        Task<Response<CompanyResponseDto>> UpdateCompanyAsync(Guid Id, UpdateCompanyRequest CompanyDto);
        Task<Response<string>> ChangeCompanyLogoAsync(Guid Id, ChangeCompanyLogoRequestDto dto);
        Task<Response<bool>> DeleteCompanyAsync(Guid Id);
    }
}
