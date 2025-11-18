using SmartCare.Application.DTOs.Inventory.Request;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IinventoryService
    {
        Task<Response<Guid>> GetBestInventoryId(Guid productId, int quantityRequired);
        Task<Response<IEnumerable<InventoryUserResponseDto>>> GetAvailableInventoriesForProduct(Guid productId);
        Task<Response<int>> GetTotalStockForProduct(Guid productId);
        Task<Response<InventoryAdminResponseDto>> IncreaseProductStock(Guid InventoryId, int quantityToAdd);
        Task<Response<InventoryAdminResponseDto>> DecreaseProductStock(Guid InventoryId, int quantityToSubtract);
        Task<Response<InventoryUserResponseDto>?> GetStockOfProductInStore(Guid productId, Guid storeId);
        Task<Response<PaginatedResult<InventoryAdminResponseDto>>> GetAllInventoryInStore(Guid storeId, int pageNumber, int pageSize);
        Task<Response<InventoryAdminResponseDto>> CreateInventoryAsync(CreateInventoryRequestDto inventoryDto);
        Task<Response<InventoryAdminResponseDto>> UpdateInventoryAsync(UpdateInventoryRequestDto inventoryDto);
        Task<Response<bool>> DeleteInventoryAsync(Guid Id);
        Task<Response<bool>> ReserveStockAsync(Guid inventoryId, int quantity);
        Task<Response<bool>> ReleaseReservedStockAsync(Guid inventoryId, int quantity);
        Task<Response<bool>> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity);
        Task<Response<IEnumerable<InventoryAdminResponseDto>>> GetLowStockItemsAsync(int threshold);
        Task<Response<IEnumerable<InventoryAdminResponseDto>>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId);
        Task<Response<bool>> SetStockLevelAsync(Guid inventoryId, int newQuantity);
    }
}
