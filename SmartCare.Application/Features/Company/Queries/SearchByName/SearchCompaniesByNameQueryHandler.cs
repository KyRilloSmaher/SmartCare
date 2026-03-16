using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Company.Queries.SearchByName
{
    public class SearchCompaniesByNameQueryHandler: IRequestHandler<SearchCompaniesByNameQuery, Response<IEnumerable<CompanyResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchCompaniesByNameQueryHandler> _logger;
        #endregion

        public SearchCompaniesByNameQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SearchCompaniesByNameQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<CompanyResponseDto>>> Handle(SearchCompaniesByNameQuery request,CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.name))
                    return _responseHandler.BadRequest<IEnumerable<CompanyResponseDto>>("Name cannot be empty.");

                var companies = await _unitOfWork
                    .Companies
                    .SearchCompaniesByNameAsync(request.name);

                var companiesDto = _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);

                return _responseHandler.Success(companiesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching companies by name {Name}", request.name);
                return _responseHandler.Failed<IEnumerable<CompanyResponseDto>>(SystemMessages.FAILED);
            }
        }
    }
}