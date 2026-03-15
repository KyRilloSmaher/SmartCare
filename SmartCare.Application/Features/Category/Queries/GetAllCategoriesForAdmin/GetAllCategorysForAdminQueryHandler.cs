using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Exceptions;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Queries.GetAllCategoriesForAdmin
{
    public class GetAllCategorysForAdminQueryHandler : IRequestHandler<GetAllCategoriesForAdminQuery, Response<IEnumerable<CategoryResponseForAdminDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCategorysForAdminQueryHandler> _logger;
        private readonly string tag = CacheConstants.Categories;
        #endregion

        public GetAllCategorysForAdminQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            ILogger<GetAllCategorysForAdminQueryHandler> logger)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<IEnumerable<CategoryResponseForAdminDto>>> Handle(GetAllCategoriesForAdminQuery request, CancellationToken cancellationToken)
        {

            string cacheKey = CacheConstants.CategoriesAllAdmin;
            IEnumerable<CategoryResponseForAdminDto>? cached = null;

            try
            {
                cached = await _redisCacheService
                    .GetDataAsync<IEnumerable<CategoryResponseForAdminDto>>(cacheKey, tag);
            }
            catch
            {
                _logger.LogError("Cache Service Error Occurred In Get All Categories For Admin QueryHandler.");
            }

            if (cached == null || !cached.Any())
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();

                var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseForAdminDto>>(categories);

                await _redisCacheService.SetDataAsync(cacheKey, categoriesDto, tag, Time.Default);

                cached = categoriesDto;
            }

            return _responseHandler.Success(cached);
        }
    }
}