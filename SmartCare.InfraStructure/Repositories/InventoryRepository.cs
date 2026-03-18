using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Exceptions;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
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
            return _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .Where(i => i.StoreId == storeId)
                .AsQueryable();
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

        public async Task<bool> FinalizeStockDeductionAsync(Guid inventoryId, int quantity, bool pickUp = false)
        {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == inventoryId);

                if (inventory == null)
                    throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

                if (inventory.ReservedQuantity < quantity)
                    throw new InvalidOperationException("Reserved quantity is insufficient.");

                if (inventory.StockQuantity < quantity)
                    throw new InvalidOperationException("Stock quantity is insufficient.");

                inventory.StockQuantity -= quantity;
                if (pickUp)
                    inventory.ReservedQuantity -= quantity;

                return true;
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
        public void CreateInventoryRecordsForBranchBulkAsync(Guid branchId)
        {
            using var transaction =  _context.Database.BeginTransaction();

            try
            {
                // Get all products
                var products =  _context.Products
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.ProductId)
                    .ToList();

                // Create inventory objects
                var inventoryRecords = products.Select(product => new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product,
                    ReservedQuantity = 0, StockQuantity = 0,
                    StoreId = branchId
                }).ToList();

                // Bulk insert (much faster for large datasets)
                 _context.BulkInsert(inventoryRecords, new BulkConfig
                {
                    BatchSize = 4000,
                    UseTempDB = true,
                    TrackingEntities = false
                });

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw new DomainException("Exception Ocuured While Proccessing Bulk INSERTION from Inventory Repo");
            }
        }
        public IQueryable<LowStockProductDto> GetLowStockProductsAsync(Guid? storeId,int threshold)
        {
            var query = _context.Inventories
                                    .Include(ps => ps.Product)
                                    .Include(ps => ps.Store)
                                    .AsQueryable();

            if (storeId.HasValue && storeId != default)
                query = query.Where(ps => ps.StoreId == storeId.Value);

            return query
                .Where(ps => Math.Abs(ps.StockQuantity - ps.ReservedQuantity) <= threshold)
                .Select(ps => new LowStockProductDto
                {
                    ProductId = ps.ProductId,
                    ProductName = ps.Product.NameEn,
                    StoreId = ps.StoreId,
                    StoreName = ps.Store.Name,
                    CurrentStock = ps.StockQuantity,
                    Threshold = threshold
                })
                .OrderBy(ps => ps.CurrentStock);
        }
        #endregion
    }
}