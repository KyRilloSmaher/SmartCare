using SmartCare.Domain.Entities;
using System.Linq.Expressions;

namespace SmartCare.Domain.IRepositories
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        #region Query Methods

        /// <summary>
        /// Gets queryable for inventories with optional tracking
        /// </summary>
        IQueryable<Inventory> GetInventoriesQueryable(bool trackChanges = false);

        /// <summary>
        /// Gets the best available inventory for a product based on available stock
        /// </summary>
        Task<Inventory?> GetAvailableInventoryAsync(Guid productId, int quantityRequired);

        /// <summary>
        /// Gets all available inventories for a product
        /// </summary>
        Task<IEnumerable<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId);

        /// <summary>
        /// Gets total available stock for a product across all inventories
        /// </summary>
        Task<int> GetTotalStockForProductAsync(Guid productId);

        /// <summary>
        /// Gets stock of a product in a specific store
        /// </summary>
        Task<Inventory?> GetStockOfProductInStoreAsync(Guid productId, Guid storeId, int quantity = 1);

        /// <summary>
        /// Gets all inventories in a store
        /// </summary>
        IQueryable<Inventory> GetAllInventoryInStoreAsync(Guid storeId);

        /// <summary>
        /// Gets low stock items across all stores
        /// </summary>
        Task<List<Inventory>> GetLowStockItemsAsync(int threshold);

        /// <summary>
        /// Gets low stock items in a specific store
        /// </summary>
        Task<List<Inventory>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId);

        /// <summary>
        /// Get Inventory By ProductId and storeId
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<Inventory?> GetInventoryByStoreAndProductAsync(Guid storeId, Guid productId);
        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Reserves stock from an inventory
        /// </summary>
        Task<bool> ReserveStockAsync(Guid inventoryId, int quantity);

        /// <summary>
        /// Releases reserved stock back to inventory
        /// </summary>
        Task<bool> ReleaseReservedStockAsync(Guid inventoryId, int quantity);

        /// <summary>
        /// Finalizes stock deduction after order completion
        /// </summary>
        Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity, bool pickUp = false);

        /// <summary>
        /// Transfers stock between inventories
        /// </summary>
        Task<bool> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity);

        /// <summary>
        /// Sets new stock level for an inventory
        /// </summary>
        Task<bool> SetStockLevelAsync(Guid inventoryId, int newQuantity);

        #endregion
    }
}