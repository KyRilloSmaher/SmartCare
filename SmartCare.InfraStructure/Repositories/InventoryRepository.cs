using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using SmartCare.Application.Handlers.ResponseHandler;
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

        public async Task<IEnumerable<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId)
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
                                           .Include(x => x.Product)
                                           .Include(x => x.Store)
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
                                         .Include(x => x.Product)
                                           .Include(x => x.Store)
                           .FirstOrDefaultAsync(i =>i.Id == InventoryId);

            if (inventory == null)
            {
                throw new InvalidOperationException("Inventory record not found.");
            }

            inventory.StockQuantity -= quantityToSubtract;

            await _context.SaveChangesAsync();

            return inventory;

        }

        public async Task<Inventory?> GetStockOfProductInStore(Guid productId, Guid storeId , int quantity = 1)
        {
            return await _context.Inventories
                              .Include(x => x.Product)
                              .Include(x => x.Store)
                .AsTracking()
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId &&( i.StockQuantity - i.ReservedQuantity) >= quantity);

        }

        public async Task<IQueryable<Inventory>> GetAllInventoryInStoreAsync(Guid storeId)
        {
            var Inventories =  _context.Inventories
                                   .Include(x => x.Product)
                                   .Include(x => x.Store)
                                .Where(i => i.StoreId == storeId).AsQueryable();
            return Inventories;
        }

        public async Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity , bool PickUp = false)
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
            inventory.StockQuantity -= quantity;
            if (PickUp)
                inventory.ReservedQuantity -= quantity;

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

        public async Task<bool> ReserveStockAsync(Guid inventoryId, int quantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");


            var availableQuantity = inventory.StockQuantity - inventory.ReservedQuantity; 

            if(quantity > availableQuantity)
            {
                throw new InvalidOperationException($"Quantity exceed from availableQuantity");
            }
            inventory.ReservedQuantity += quantity;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ReleaseReservedStockAsync(Guid inventoryId, int quantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");


            inventory.ReservedQuantity -= quantity;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity)
        {
            var frominventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == fromInventoryId);
            var toinventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == toInventoryId);
            if (frominventory == null)
                throw new InvalidOperationException($"Inventory with ID {fromInventoryId} not found.");
            if (toinventory == null)
                throw new InvalidOperationException($"Inventory with ID {toInventoryId} not found.");
            frominventory.StockQuantity -= quantity;
            toinventory.StockQuantity += quantity;

            var result = await _context.SaveChangesAsync();
            return result > 0;

        }

        public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold)
        {
            List<Inventory> LowStock = new List<Inventory>();

            var inventories = await _context.Inventories
                                .Include(x => x.OrderItems)
                                .Include(x => x.Product)
                                .Include(x => x.Store)
                                .Where(I => I.StockQuantity <= threshold).ToListAsync();

            if(!inventories.Any())
                return LowStock;

                var ProductIds = inventories.Select(i => i.ProductId).ToList();

            var OrderItems = await _context.OrderItems
                                      .Where(O => ProductIds.Contains(O.ProductId))
                                      .GroupBy(i => i.ProductId)
                                      .Select(g => new
                                      {
                                          ProductId = g.Key,
                                          TotalOrdered = g.Sum(x => x.Quantity)
                                      }).ToListAsync();

            var orderItemDictinory = OrderItems.ToDictionary(x => x.ProductId, x => x.TotalOrdered);
            foreach (var inv in inventories)
            {
                int ordered = orderItemDictinory.ContainsKey(inv.ProductId) ? orderItemDictinory[inv.ProductId] : 0;

                int totalStock = inv.StockQuantity + ordered;

                if(totalStock == 0)
                    continue;

                double ratio = (double)ordered / totalStock;
                
                if(ratio >= 0.5)
                    LowStock.Add(inv);
            }


           return LowStock;

        }
        public async Task<bool> SetStockLevelAsync(Guid inventoryId, int newQuantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

            if (inventory.ReservedQuantity > newQuantity)
                throw new InvalidOperationException("Error: Reserved quantity exceeds the new avaliable stock.");

            inventory.StockQuantity = newQuantity;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        public async Task<List<Inventory>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId)
        {
            List<Inventory> LowStock = new List<Inventory>();

            var inventories = await _context.Inventories
                                .Include(x => x.Product)
                                .Include(x => x.Store)
                                .Where(I => I.StockQuantity <= threshold &&  I.StoreId == storeId).ToListAsync();

            if (!inventories.Any())
                return LowStock;

            var ProductIds = inventories.Select(i => i.ProductId).ToList();

            var OrderItems = await _context.OrderItems
                                      .Include(o => o.Inventory)
                                      .Where(O => ProductIds.Contains(O.ProductId) && O.Inventory.StoreId == storeId)
                                      .GroupBy(i => i.ProductId)
                                      .Select(g => new
                                      {
                                          ProductId = g.Key,
                                          TotalOrdered = g.Sum(x => x.Quantity)
                                      }).ToListAsync();

            var orderItemDictinory = OrderItems.ToDictionary(x => x.ProductId, x => x.TotalOrdered);
            
            foreach (var inv in inventories)
            {
                int ordered = orderItemDictinory.ContainsKey(inv.ProductId) ? orderItemDictinory[inv.ProductId] : 0;

                int totalStock = inv.StockQuantity + ordered;

                if (totalStock == 0)
                    continue;

                double ratio = (double)ordered / totalStock;

                if (ratio >= 0.5)
                    LowStock.Add(inv);
            }

            return LowStock;
        }
        #endregion

        public override async Task<Inventory> AddAsync(Inventory inventory)
        {
            if (inventory.ReservedQuantity > inventory.StockQuantity)
                throw new Exception("Reserved cannot exceed stock");

            var addedInventory = await base.AddAsync(inventory);

            return await _context.Inventories
                .Include(i => i.Store)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == addedInventory.Id);

        }

        public async Task<bool> DeleteAsync(Guid Id)
        {
            var inventory = await _context.Inventories
                             .Where(x => x.Id == Id)
                             .FirstOrDefaultAsync();
            if (inventory == null)
                return false;

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Inventory> UpdateinventoryAsync(Guid Id ,int StockQuantity , int ReservedQuantity)
        {
            var inventory = await _context.Inventories
                              .Where(x => x.Id == Id)
                              .FirstOrDefaultAsync();

            if (inventory == null)
                throw new Exception("Inventory not found.");

            inventory.StockQuantity = StockQuantity;
            inventory.ReservedQuantity = ReservedQuantity;

            await _context.SaveChangesAsync();
            return await _context.Inventories
                .Include(i => i.Store)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == Id);

        }
    
    }
}
