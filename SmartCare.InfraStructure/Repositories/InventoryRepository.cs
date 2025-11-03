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
        public Task<Guid> GetBestInventoryIdAsync(Guid productId, int quantityRequired)
        {
            throw new NotImplementedException();
        }

        public Task<Inventory> GetAvalaibleStockForProductAsync(Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalStockForProductAsync(Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<Inventory> IncreaseProductStockAsync(Guid InventoryId, int quantityToAdd)
        {
            throw new NotImplementedException();
        }

        public Task<Inventory> DecreaseProductStockAsync(Guid InventoryId, int quantityToSubtract)
        {
            throw new NotImplementedException();
        }

        public Task<Inventory> GetStockOfProductInStore(Guid productId, Guid storeId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Inventory>> GetAllInventoryInStoreAsync(Guid storeId)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
