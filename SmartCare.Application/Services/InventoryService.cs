using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Inventory.Request;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartCare.Application.Services
{
    public class InventoryService : IinventoryService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public InventoryService(IResponseHandler responseHandler,
                                IInventoryRepository inventoryRepository,
                                IProductRepository productRepository,
                                IStoreRepository storeRepository, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }


        #endregion

        #region Methods
        public async Task<Response<Guid>> GetBestInventoryId(Guid productId, int quantityRequired)
        {
            if (productId == Guid.Empty && quantityRequired <= 0)
                return _responseHandler.BadRequest<Guid>(SystemMessages.INVALID_INPUT);
            var inventoryId = await _inventoryRepository.GetBestInventoryIdAsync(productId, quantityRequired);
            if (inventoryId == Guid.Empty)
                return _responseHandler.Failed<Guid>(SystemMessages.NOT_FOUND);
            return _responseHandler.Success(inventoryId);
        }

        public async Task<Response<IEnumerable<InventoryUserResponseDto>>> GetAvailableInventoriesForProduct(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<IEnumerable<InventoryUserResponseDto>>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<IEnumerable<InventoryUserResponseDto>>(SystemMessages.INVALID_INPUT);
            var inventories = await _inventoryRepository.GetAvailableInventoriesForProductAsync(productId);
            if (inventories == null)
                return _responseHandler.Failed<IEnumerable<InventoryUserResponseDto>>(SystemMessages.NOT_FOUND);
            var inventoryDtoList = _mapper.Map<IEnumerable<InventoryUserResponseDto>>(inventories.ToList());
            return _responseHandler.Success(inventoryDtoList);
        }

        public async Task<Response<int>> GetTotalStockForProduct(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<int>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty)
                return _responseHandler.BadRequest<int>(SystemMessages.INVALID_INPUT);

            var totalStock = await _inventoryRepository.GetTotalStockForProductAsync(productId);
            return _responseHandler.Success(totalStock);

        }

        public async Task<Response<InventoryAdminResponseDto>> IncreaseProductStock(Guid InventoryId, int quantityToAdd)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(InventoryId);
            if (inventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (InventoryId == Guid.Empty && quantityToAdd <= 0)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);
            var Inventory = await _inventoryRepository.IncreaseProductStockAsync(InventoryId,quantityToAdd);
            if (Inventory == null)
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.NOT_FOUND);
            var inventoryDto = _mapper.Map<InventoryAdminResponseDto>(Inventory);
            return _responseHandler.Success(inventoryDto);
        }

        public async Task<Response<InventoryAdminResponseDto>> DecreaseProductStock(Guid InventoryId, int quantityToSubtract)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(InventoryId);
            if (inventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.INVENTORY_NOT_FOUND);
            }
            if (InventoryId == Guid.Empty && quantityToSubtract <= 0)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);
            var Inventory = await _inventoryRepository.DecreaseProductStockAsync(InventoryId,quantityToSubtract);
            if (Inventory == null)
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.NOT_FOUND);
            var inventoryDto = _mapper.Map<InventoryAdminResponseDto>(Inventory);
            return _responseHandler.Success(inventoryDto);
        }

        public async Task<Response<InventoryUserResponseDto>?> GetStockOfProductInStore(Guid productId, Guid storeId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }
            if (productId == Guid.Empty && storeId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryUserResponseDto>(SystemMessages.INVALID_INPUT);
            var inventory = await _inventoryRepository.GetStockOfProductInStore(productId,storeId);
            if (inventory == null)
                return _responseHandler.Failed<InventoryUserResponseDto>(SystemMessages.NOT_FOUND);
            var inventoryDto = _mapper.Map<InventoryUserResponseDto>(inventory);
            return _responseHandler.Success(inventoryDto);
        }

        public async Task<Response<PaginatedResult<InventoryAdminResponseDto>>> GetAllInventoryInStore(Guid storeId, int pageNumber, int pageSize)
        {
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

        public async Task<Response<bool>> ReserveStockAsync(Guid inventoryId, int quantity)
        {
            if (inventoryId == Guid.Empty && quantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var result = await _inventoryRepository.ReserveStockAsync(inventoryId,quantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<bool>> ReleaseReservedStockAsync(Guid inventoryId, int quantity)
        {
            if (inventoryId == Guid.Empty && quantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var result = await _inventoryRepository.ReleaseReservedStockAsync(inventoryId, quantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<bool>> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity)
        {
            if (fromInventoryId == Guid.Empty && toInventoryId == Guid.Empty && quantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var Frominventory = await _inventoryRepository.GetByIdAsync(fromInventoryId);
            if (Frominventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }
            var Toinventory = await _inventoryRepository.GetByIdAsync(toInventoryId);
            if (Toinventory == null)
            {
                return _responseHandler.Failed<bool>(SystemMessages.INVENTORY_NOT_FOUND);
            }
            var AvailableQuantity = Frominventory.StockQuantity - Frominventory.ReservedQuantity;
            
            if(quantity > AvailableQuantity)
                throw new InvalidOperationException($"Quantity exceed from AvailbleQuantity");
            var result = await _inventoryRepository.TransferStockAsync(fromInventoryId , toInventoryId , quantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<IEnumerable<InventoryAdminResponseDto>>> GetLowStockItemsAsync(int threshold)
        {
           
            if (threshold < 0)
                return _responseHandler.BadRequest<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.INVALID_INPUT);
            var inventories = await _inventoryRepository.GetLowStockItemsAsync(threshold);
            if (inventories == null)
                return _responseHandler.Failed<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.NOT_FOUND);
            var InventoryDto = _mapper.Map<IEnumerable<InventoryAdminResponseDto>>(inventories);
            return _responseHandler.Success(InventoryDto);
        }

        public async Task<Response<IEnumerable<InventoryAdminResponseDto>>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId)
        {
            
            if (storeId == Guid.Empty && threshold < 0)
                return _responseHandler.BadRequest<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.INVALID_INPUT);
            var inventories = await _inventoryRepository.GetLowStockItemsInStoreAsync(threshold,storeId);
            if (inventories == null)
                return _responseHandler.Failed<IEnumerable<InventoryAdminResponseDto>>(SystemMessages.NOT_FOUND);
            var InventoryDto = _mapper.Map<IEnumerable<InventoryAdminResponseDto>>(inventories);
            return _responseHandler.Success(InventoryDto);
        }

        public async Task<Response<bool>> SetStockLevelAsync(Guid inventoryId, int newQuantity)
        {
            if (inventoryId == Guid.Empty && newQuantity <= 0)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var result = await _inventoryRepository.SetStockLevelAsync(inventoryId , newQuantity);
            return result ? _responseHandler.Success(true, SystemMessages.INVENTORY_UPDATED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }

        public async Task<Response<InventoryAdminResponseDto>> CreateInventoryAsync(CreateInventoryRequestDto inventoryDto)
        {
            var product = await _productRepository.GetByIdAsync(inventoryDto.ProductId);
            if (product == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.PRODUCT_NOT_FOUND);
            }

            var store = await _storeRepository.GetByIdAsync(inventoryDto.StoreId);
            if (store == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.STORE_NOT_FOUND);
            }
            var inventory = _mapper.Map<Inventory>(inventoryDto);
            var savedInventory = await _inventoryRepository.AddAsync(inventory);
            var InventoryDto = _mapper.Map<InventoryAdminResponseDto>(savedInventory);
            return _responseHandler.Success(InventoryDto);
        }

        public async Task<Response<InventoryAdminResponseDto>> UpdateInventoryAsync(UpdateInventoryRequestDto inventoryDto)
        {
            if (inventoryDto.InventoryId == Guid.Empty)
                return _responseHandler.BadRequest<InventoryAdminResponseDto>(SystemMessages.INVALID_INPUT);

            var existinginventory = await _inventoryRepository.GetByIdAsync(inventoryDto.InventoryId);
            if (existinginventory == null)
            {
                return _responseHandler.Failed<InventoryAdminResponseDto>(SystemMessages.INVENTORY_NOT_FOUND);
            }

            _mapper.Map(inventoryDto, existinginventory);
            var updatedinventory = await _inventoryRepository.UpdateinventoryAsync(inventoryDto.InventoryId , inventoryDto.StockQuantity , inventoryDto.ReservedQuantity);
            await _inventoryRepository.SaveChangesAsync();
            var updatedInventoryDto = _mapper.Map<InventoryAdminResponseDto>(updatedinventory);
            return _responseHandler.Success(updatedInventoryDto, SystemMessages.RECORD_UPDATED);

        }

        public async Task<Response<bool>> DeleteInventoryAsync(Guid Id)
        {
            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);
            var inventory = await _inventoryRepository.GetByIdAsync(Id);
            if (inventory == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);
            var result = await _inventoryRepository.DeleteAsync(inventory);
            return result ? _responseHandler.Success(true, SystemMessages.RECORD_DELETED) : _responseHandler.Failed<bool>(SystemMessages.FAILED);
        }
        #endregion
    }
}
