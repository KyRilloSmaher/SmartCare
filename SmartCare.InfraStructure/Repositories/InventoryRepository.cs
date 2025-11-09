using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class InventoryRepository : GenericRepository<Inventory>  , IInventoryRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion
        #region Constructor
        public InventoryRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Methods
        public async Task<Guid> GetBestInventoryIdAsync(Guid productId, int quantityRequired)
        {
            var inventory = await _context.Inventories
                         .Where(i => i.ProductId == productId)
                         .OrderByDescending(i => i.StockQuantity)
                         .FirstOrDefaultAsync();

            return inventory.Id;
        }

        public async Task<List<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId)
        {
            var inventories = await _context.Inventories
                          .Where(i => i.ProductId == productId)
                          .ToListAsync();

            return inventories;
        }

        public async Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            var Total = await _context.Inventories
                              .Where(i => i.ProductId == productId)
                              .SumAsync(i => i.StockQuantity);
            return Total;
        }

        public async Task<Inventory> IncreaseProductStockAsync(Guid InventoryId, Guid productId, int quantityToAdd)
        {
            var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == InventoryId);

            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            inventory.StockQuantity += quantityToAdd;

            await _context.SaveChangesAsync();

            return inventory;

        }

        public async Task<Inventory> DecreaseProductStockAsync(Guid InventoryId, Guid productId, int quantityToSubtract)
        {
            var inventory = await _context.Inventories
                           .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == InventoryId);

            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            inventory.StockQuantity -= quantityToSubtract;

            await _context.SaveChangesAsync();

            return inventory;

        }

        public async Task<Inventory> GetStockOfProductInStore(Guid productId, Guid storeId)
        {
            var inventory = await _context.Inventories
                         .Where(i => i.ProductId == productId && i.StoreId == storeId)
                         .FirstOrDefaultAsync();

            return inventory;
        }

        public async Task<List<Inventory>> GetAllInventoryInStoreAsync(Guid storeId)
        {
            var Inventories = await _context.Inventories
                                .Where(i => i.StoreId == storeId).ToListAsync();

            if (Inventories == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            return Inventories;
        }

        public async Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, Guid productId, int quantity)
        {
            var inventory = await _context.Inventories
                                 .Where(i => i.ProductId == productId && i.Id == inventoryId)
                                 .FirstOrDefaultAsync();
            var stockIninventory = inventory.StockQuantity;
            if (stockIninventory < quantity)
                return false;
            return true;
        }

        public async Task<bool> FinalizeStockDeductionForProductAsync(Guid productId, int quantity)
        {
            var sumOfstock = await _context.Inventories
                                 .Where(i => i.ProductId == productId)
                                 .SumAsync(i => i.StockQuantity);
            if (sumOfstock < quantity)
                return false;
            return true;
        }
        #endregion
    }
}
