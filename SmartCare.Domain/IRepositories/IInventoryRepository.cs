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
        Task<IEnumerable<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId);
        Task<int> GetTotalStockForProductAsync(Guid productId);
        Task<Inventory> IncreaseProductStockAsync(Guid InventoryId , int quantityToAdd);
        Task<Inventory> DecreaseProductStockAsync(Guid InventoryId , int quantityToSubtract);
        Task<Inventory?> GetStockOfProductInStore(Guid productId, Guid storeId , int quantity = 1);
        Task<IQueryable<Inventory>> GetAllInventoryInStoreAsync(Guid storeId);


        /// <summary>
        /// Finalizes stock deduction for an inventory item after order confirmation
        /// This permanently deducts stock that was previously reserved
        /// </summary>
        /// <param name="inventoryId">The inventory ID to deduct from</param>
        /// <param name="quantity">The quantity to deduct</param>
        /// <param name="PickUp">In Case its A pickUp Order</param>
        /// <returns>True if successful, false if insufficient stock or not found</returns>
        Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity, bool PickUp = false);

        /// <summary>
        /// Finalizes stock deduction for a specific product across any inventory
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="quantity">The quantity to deduct</param>
        /// <returns>True if successful, false if insufficient stock</returns>
        Task<bool> FinalizeStockDeductionForProductAsync(Guid productId, int quantity);

        Task<bool> ReserveStockAsync(Guid inventoryId, int quantity);
        Task<bool> ReleaseReservedStockAsync(Guid inventoryId, int quantity);
        Task<bool> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity);
        Task<List<Inventory>> GetLowStockItemsAsync(int threshold);
        Task<List<Inventory>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId);

        Task<bool> SetStockLevelAsync(Guid inventoryId, int newQuantity);
        Task<Inventory> UpdateinventoryAsync(Guid Id, int StockQuantity, int ReservedQuantity);
    }
}
