using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _CompanyService;

        public CompanyController(ICompanyService CompanyService)
        {
            _CompanyService = CompanyService;
        }

        /// <summary>
        /// Get Company By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Company.GetCompanyById)]
        [ProducesResponseType(typeof(Response<CompanyResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompanyByIdAsync(Guid id)
        {
            var result = await _CompanyService.GetCompanyByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Companies By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Company.SearchCompanyByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCompaniesByNameAsync([FromQuery]string name)
        {
            var result = await _CompanyService.SearchCompaniesByNameAsync(name);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Companies
        /// </summary>
        [HttpGet(ApplicationRouting.Company.GetAllCompanies)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompaniesAsync()
        {
            var result = await _CompanyService.GetAllCompaniesAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Companies (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(ApplicationRouting.Company.GetAllCompaniesForAdmin)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseForAdminDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompaniesForAdminAsync()
        {
            var result = await _CompanyService.GetAllCompaniesForAdminAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a New Company
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost(ApplicationRouting.Company.CreateCompany)]
        [ProducesResponseType(typeof(Response<CompanyResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCompanyAsync([FromForm] CreateCompanyRequestDto dto)
        {
            var result = await _CompanyService.CreateCompanyAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Company
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut(ApplicationRouting.Company.UpdateCompany)]
        [ProducesResponseType(typeof(Response<CompanyResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCompanyAsync(Guid id, [FromBody] UpdateCompanyRequest dto)
        {
            var result = await _CompanyService.UpdateCompanyAsync(id, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Change Company Logo
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch(ApplicationRouting.Company.ChangeCompanyImage)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeCompanyLogoAsync(Guid id, [FromForm] ChangeCompanyLogoRequestDto dto)
        {
            var result = await _CompanyService.ChangeCompanyLogoAsync(id, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Company
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete(ApplicationRouting.Company.DeleteCompany)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCompanyAsync(Guid id)
        {
            var result = await _CompanyService.DeleteCompanyAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
