using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Store
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsDeleted { get; set; } = false;
        public  ICollection<Order> Orders { get; set; }
        public  ICollection<Inventory> Inventories { get; set; }

    }
}
