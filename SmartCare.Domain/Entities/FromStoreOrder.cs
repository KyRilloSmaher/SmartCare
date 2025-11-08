using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class FromStoreOrder : Order
    {
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
    }
}
