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
                            .Where(i => i.ProductId == productId && (i.StockQuantity - i.ReservedQuantity) >= quantityRequired)
                            .OrderByDescending(i => i.StockQuantity - i.ReservedQuantity)
                            .FirstOrDefaultAsync();
            if (inventory == null || inventory.StockQuantity < quantityRequired)
                return Guid.Empty;

            return inventory.Id;
        }

        public async Task<List<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId)
        {
            var inventories = await _context.Inventories.Include(i => i.Store).Include(i => i.Product)
                          .Where(i => i.ProductId == productId)
                          .ToListAsync();

            return inventories;
        }

        public async Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            var Total = await _context.Inventories
                              .Where(i => i.ProductId == productId)
                              .SumAsync(i => i.StockQuantity - i.ReservedQuantity);
            return Total;
        }

        public async Task<Inventory> IncreaseProductStockAsync(Guid InventoryId, int quantityToAdd)
        {
            var inventory = await _context.Inventories
                                          .FirstOrDefaultAsync(i => i.Id == InventoryId);

            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            inventory.StockQuantity += quantityToAdd;

            await _context.SaveChangesAsync();

            return inventory;

        }

        public async Task<Inventory> DecreaseProductStockAsync(Guid InventoryId , int quantityToSubtract)
        {
            var inventory = await _context.Inventories
                           .FirstOrDefaultAsync(i =>i.Id == InventoryId);

            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            inventory.StockQuantity -= quantityToSubtract;

            await _context.SaveChangesAsync();

            return inventory;

        }

        public async Task<Inventory?> GetStockOfProductInStore(Guid productId, Guid storeId)
        {
            return await _context.Inventories
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId);

        }

        public async Task<List<Inventory>> GetAllInventoryInStoreAsync(Guid storeId)
        {
            var Inventories = await _context.Inventories
                                .Where(i => i.StoreId == storeId).ToListAsync();
            return Inventories;
        }

        public async Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity)
        {

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

            // Ensure sufficient reserved and total stock
            if (inventory.ReservedQuantity < quantity)
                throw new InvalidOperationException("Cannot finalize deduction. Reserved quantity is insufficient.");

            if (inventory.StockQuantity < quantity)
                throw new InvalidOperationException("Cannot finalize deduction. Stock quantity is insufficient.");

            // Perform deduction
            inventory.ReservedQuantity -= quantity;
            inventory.StockQuantity -= quantity;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }


        public async Task<bool> FinalizeStockDeductionForProductAsync(Guid productId, int quantity)
        {

            // Get all inventories that contain reserved stock for this product
            var inventories = await _context.Inventories
                .Where(i => i.ProductId == productId && i.ReservedQuantity > 0)
                .OrderByDescending(i => i.ReservedQuantity)
                .ToListAsync();

            if (inventories == null || inventories.Count == 0)
                throw new InvalidOperationException("No inventories found with reserved stock for this product.");

            int totalReserved = inventories.Sum(i => i.ReservedQuantity);
            if (totalReserved < quantity)
                throw new InvalidOperationException("Insufficient reserved stock across all inventories to finalize deduction.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int remainingToDeduct = quantity;

                foreach (var inventory in inventories)
                {
                    if (remainingToDeduct <= 0)
                        break;

                    int deductQuantity = Math.Min(inventory.ReservedQuantity, remainingToDeduct);

                    if (inventory.StockQuantity < deductQuantity)
                        throw new InvalidOperationException($"Inventory {inventory.Id} has insufficient stock to deduct.");

                    inventory.ReservedQuantity -= deductQuantity;
                    inventory.StockQuantity -= deductQuantity;

                    remainingToDeduct -= deductQuantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}
