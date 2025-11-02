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
    public class OrderRepository : GenericRepository<Order> ,IOrderRepository
    {
        #region Feilds
        public readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public OrderRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Methods
        #endregion
    }
}
