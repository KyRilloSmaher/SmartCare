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
    public class AdressRepository : GenericRepository<Address>, IAdressRepository
    {
        private readonly ApplicationDBContext _context;

        public AdressRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;

        }


    }
}
