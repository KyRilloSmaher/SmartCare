using AutoMapper;
using MediatR;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Category.Queries.SearchForCatgeory
{
    public class SearchCategoriesByNameQueryHandler : IRequestHandler<SearchCategoriesByNameQuery, Response<IEnumerable<CategoryResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public SearchCategoriesByNameQueryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<CategoryResponseDto>>> Handle(SearchCategoriesByNameQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Categories.SearchCategoryByNameAsync(request.name);
            var categoriesDto = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            return _responseHandler.Success(categoriesDto);
        }
    }
}