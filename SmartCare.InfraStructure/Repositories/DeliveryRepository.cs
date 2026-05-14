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
    public class DeliveryRepository : GenericRepository<Delivery> , IDeliveryRepository
    {
        public DeliveryRepository (ApplicationDBContext context) :base(context) { }

    }
}
