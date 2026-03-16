using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Features.Company.Commands.ChangeLogo;
using SmartCare.Application.Features.Company.Commands.Create;
using SmartCare.Application.Features.Company.Commands.Delete;
using SmartCare.Application.Features.Company.Commands.Restore;
using SmartCare.Application.Features.Company.Commands.Update;
using SmartCare.Application.Features.Company.Queries.GetAll;
using SmartCare.Application.Features.Company.Queries.GetAllByPaginated;
using SmartCare.Application.Features.Company.Queries.GetAllForAdmin;
using SmartCare.Application.Features.Company.Queries.GetById;
using SmartCare.Application.Features.Company.Queries.SearchByName;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompanyController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get Company By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Company.GetById)]
        [ProducesResponseType(typeof(Response<CompanyResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompanyByIdAsync(Guid id)
        {
            var result = await _mediator.Send(new GetCompanyByIdQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Companies By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Company.SearchByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCompaniesByNameAsync([FromQuery]string name)
        {
            var result = await _mediator.Send(new SearchCompaniesByNameQuery(name));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Companies
        /// </summary>
        [HttpGet(ApplicationRouting.Company.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompaniesAsync()
        {
            var result = await _mediator.Send(new GetAllCompaniesQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Companies By Pagination
        /// </summary>
        [HttpGet(ApplicationRouting.Company.GetAllPaginated)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPaginatedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _mediator.Send(new GetAllCompaniesPaginatedQuery(pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Companies (Admin)
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpGet(ApplicationRouting.Company.GetAllForAdmin)]
        [ProducesResponseType(typeof(Response<IEnumerable<CompanyResponseForAdminDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompaniesForAdminAsync()
        {
            var result = await _mediator.Send(new GetAllCompaniesForAdminQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a New Company
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPost(ApplicationRouting.Company.Create)]
        [ProducesResponseType(typeof(Response<CompanyResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCompanyAsync([FromForm] CreateCompanyRequestDto dto)
        {
            var result = await _mediator.Send(new CreateCompanyCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Company
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPut(ApplicationRouting.Company.Update)]
        [ProducesResponseType(typeof(Response<CompanyResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCompanyAsync([FromBody] UpdateCompanyRequest dto)
        {;
            var result = await _mediator.Send(new UpdateCompanyCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Change Company Logo
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPatch(ApplicationRouting.Company.ChangeImage)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeCompanyLogoAsync([FromForm] ChangeCompanyLogoRequestDto dto)
        {
            var result = await _mediator.Send(new ChangeCompanyLogoCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Restore Company
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPatch(ApplicationRouting.Company.Restore)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreCompanyAsync(Guid id)
        {
            var result = await _mediator.Send(new RestoreCompanyCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Delete Company
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Company.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCompanyAsync(Guid id)
        {
            var result = await _mediator.Send(new  DeleteCompanyCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
