using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        Task<Guid> GetBestInventoryIdAsync(Guid productId ,int quantityRequired);
        Task<List<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId);
        Task<int> GetTotalStockForProductAsync(Guid productId);
        Task<Inventory> IncreaseProductStockAsync(Guid InventoryId, Guid productId, int quantityToAdd);
        Task<Inventory> DecreaseProductStockAsync(Guid InventoryId, Guid productId, int quantityToSubtract);
        Task<Inventory> GetStockOfProductInStore(Guid productId, Guid storeId);
        Task<List<Inventory>> GetAllInventoryInStoreAsync(Guid storeId);
        Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, Guid productId, int quantity);
        Task<bool> FinalizeStockDeductionForProductAsync(Guid productId, int quantity);

    }
}
