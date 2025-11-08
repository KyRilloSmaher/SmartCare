using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class OnlineOrder : Order
    {
        public Guid AddressId { get; set; }
        public Address Address { get; set; }
    }
}
