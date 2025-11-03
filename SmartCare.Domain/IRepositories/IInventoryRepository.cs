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
        Task<Inventory> GetAvalaibleStockForProductAsync(Guid productId);
        Task<int> GetTotalStockForProductAsync(Guid productId);
        Task<Inventory> IncreaseProductStockAsync(Guid InventoryId, int quantityToAdd);
        Task<Inventory> DecreaseProductStockAsync(Guid InventoryId, int quantityToSubtract);
        Task<Inventory> GetStockOfProductInStore(Guid productId, Guid storeId);
        Task<List<Inventory>> GetAllInventoryInStoreAsync(Guid storeId);


    }
}
