using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Favourite.Queries;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class GetAllInventoryInStoreHandler : IRequestHandler<GetAllInventoryInStoreQuery, Response<PaginatedResult<InventoryAdminResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;


        #endregion

        public GetAllInventoryInStoreHandler(IResponseHandler responseHandler, IInventoryRepository inventoryRepository, IProductRepository productRepository, IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedResult<InventoryAdminResponseDto>>> Handle(GetAllInventoryInStoreQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.pageNumber;
            var pageSize = request.pageSize;
            var storeId = request.storeId;
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<InventoryAdminResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            if (storeId == Guid.Empty)
                return _responseHandler.BadRequest<PaginatedResult<InventoryAdminResponseDto>>(SystemMessages.INVALID_INPUT);

            var inventories = await _inventoryRepository.GetAllInventoryInStoreAsync(storeId);
            if (inventories == null)
                return _responseHandler.Failed<PaginatedResult<InventoryAdminResponseDto>>(SystemMessages.NOT_FOUND);
            //var inventoryDto = _mapper.Map<PaginatedResult<InventoryAdminResponseDto>>(inventories.ToList());
            var projectedQuery = _mapper.ProjectTo<InventoryAdminResponseDto>(inventories);
            var paginatedResult = await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize);
            return _responseHandler.Success(paginatedResult);
        }
    }
}
