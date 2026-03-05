using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
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

        #region Query Methods

        public IQueryable<Inventory> GetInventoriesQueryable(bool trackChanges = false)
        {
            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store);

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<Inventory?> GetAvailableInventoryAsync(Guid productId, int quantityRequired)
        {
            return await _context.Inventories
                .Where(i => i.ProductId == productId &&
                           (i.StockQuantity - i.ReservedQuantity) >= quantityRequired)
                .OrderByDescending(i => i.StockQuantity - i.ReservedQuantity)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Inventory>> GetAvailableInventoriesForProductAsync(Guid productId)
        {
            return await _context.Inventories
                .Include(i => i.Store)
                .Include(i => i.Product)
                .Where(i => i.ProductId == productId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            return await _context.Inventories
                .Where(i => i.ProductId == productId)
                .SumAsync(i => i.StockQuantity - i.ReservedQuantity);
        }

        public async Task<Inventory?> GetStockOfProductInStoreAsync(Guid productId, Guid storeId, int quantity = 1)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                         i.StoreId == storeId &&
                                         (i.StockQuantity - i.ReservedQuantity) >= quantity);
        }

        public IQueryable<Inventory> GetAllInventoryInStoreAsync(Guid storeId)
        {
<<<<<<< HEAD
            var Inventories =  _context.Inventories
                                   .Include(x => x.Product)
                                   .Include(x => x.Store)
                                .Where(i => i.StoreId == storeId).AsQueryable();
            return Inventories;
        }

        public async Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity, bool pickUp = false)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

            if (inventory.ReservedQuantity < quantity)
                throw new InvalidOperationException("Insufficient reserved quantity.");

            if (inventory.StockQuantity < quantity)
                throw new InvalidOperationException("Insufficient stock quantity.");

            inventory.StockQuantity -= quantity;

            // 🔥 IMPORTANT: DO NOT LOAD PRODUCT AGAIN
            var availableStock = await _context.Inventories
                .Where(inv => inv.ProductId == inventory.ProductId)
                .SumAsync(inv => inv.StockQuantity - inv.ReservedQuantity);

            var trackedProduct = _context.ChangeTracker
                .Entries<Product>()
                .FirstOrDefault(e => e.Entity.ProductId == inventory.ProductId)
                ?.Entity;

            if (trackedProduct != null)
            {
                trackedProduct.IsAvailable = availableStock > 0;
            }

            return await _context.SaveChangesAsync() > 0;
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

=======
            return _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .Where(i => i.StoreId == storeId)
                .AsQueryable();
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
        }

        public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold)
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .Where(i => i.StockQuantity <= threshold)
                .ToListAsync();

            if (!inventories.Any())
                return new List<Inventory>();

            var productIds = inventories.Select(i => i.ProductId).ToList();

            var orderItems = await _context.OrderItems
                .Where(oi => productIds.Contains(oi.ProductId))
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrdered = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var orderItemDict = orderItems.ToDictionary(x => x.ProductId, x => x.TotalOrdered);
            var lowStock = new List<Inventory>();

            foreach (var inv in inventories)
            {
                int ordered = orderItemDict.ContainsKey(inv.ProductId) ? orderItemDict[inv.ProductId] : 0;
                int totalStock = inv.StockQuantity + ordered;

                if (totalStock == 0)
                    continue;

                double ratio = (double)ordered / totalStock;

                if (ratio >= 0.5)
                    lowStock.Add(inv);
            }

            return lowStock;
        }

        public async Task<List<Inventory>> GetLowStockItemsInStoreAsync(int threshold, Guid storeId)
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .Where(i => i.StockQuantity <= threshold && i.StoreId == storeId)
                .ToListAsync();

            if (!inventories.Any())
                return new List<Inventory>();

            var productIds = inventories.Select(i => i.ProductId).ToList();

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Inventory)
                .Where(oi => productIds.Contains(oi.ProductId) && oi.Inventory.StoreId == storeId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrdered = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var orderItemDict = orderItems.ToDictionary(x => x.ProductId, x => x.TotalOrdered);
            var lowStock = new List<Inventory>();

            foreach (var inv in inventories)
            {
                int ordered = orderItemDict.ContainsKey(inv.ProductId) ? orderItemDict[inv.ProductId] : 0;
                int totalStock = inv.StockQuantity + ordered;

                if (totalStock == 0)
                    continue;

                double ratio = (double)ordered / totalStock;

                if (ratio >= 0.5)
                    lowStock.Add(inv);
            }

            return lowStock;
        }

        #endregion

        #region Business Logic Methods

        public Task<bool> ReserveStockAsync(Guid inventoryId, int quantity)
        {
            return Task.Run(async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory == null)
                    throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

                var availableQuantity = inventory.StockQuantity - inventory.ReservedQuantity;

                if (quantity > availableQuantity)
                    throw new InvalidOperationException($"Quantity exceeds available stock");

                inventory.ReservedQuantity += quantity;
                return true;
            });
        }

        public Task<bool> ReleaseReservedStockAsync(Guid inventoryId, int quantity)
        {
            return Task.Run(async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory == null)
                    throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

                inventory.ReservedQuantity -= quantity;
                return true;
            });
        }

        public Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity, bool pickUp = false)
        {
            return Task.Run(async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory == null)
                    throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

                if (inventory.ReservedQuantity < quantity)
                    throw new InvalidOperationException("Reserved quantity is insufficient.");

                if (inventory.StockQuantity < quantity)
                    throw new InvalidOperationException("Stock quantity is insufficient.");

                inventory.Confirm(quantity);
                return true;
            });
        }

        public Task<bool> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity)
        {
            return Task.Run(async () =>
            {
                var fromInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == fromInventoryId);
                var toInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == toInventoryId);

                if (fromInventory == null)
                    throw new InvalidOperationException($"Inventory with ID {fromInventoryId} not found.");
                if (toInventory == null)
                    throw new InvalidOperationException($"Inventory with ID {toInventoryId} not found.");

                fromInventory.StockQuantity -= quantity;
                toInventory.StockQuantity += quantity;

                return true;
            });
        }

        public Task<bool> SetStockLevelAsync(Guid inventoryId, int newQuantity)
        {
            return Task.Run(async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory == null)
                    throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

                if (inventory.ReservedQuantity > newQuantity)
                    throw new InvalidOperationException("Reserved quantity exceeds new stock level.");

                inventory.StockQuantity = newQuantity;
                return true;
            });
        }

        public override async Task<Inventory> AddAsync(Inventory inventory)
        {
            if (inventory.ReservedQuantity > inventory.StockQuantity)
                throw new Exception("Reserved cannot exceed stock");

            return await base.AddAsync(inventory);
        }

<<<<<<< HEAD
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

        public async Task<bool> IsStockAvailableAsync(Guid inventoryId, Guid productId)
        { 
           return await _context.Inventories
                .AnyAsync(i => i.Id == inventoryId && i.ProductId == productId && (i.StockQuantity - i.ReservedQuantity) > 0);
        }
=======
        #endregion
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
    }
}