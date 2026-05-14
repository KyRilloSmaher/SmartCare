using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public  class Delivery
    {
       public string Id { get; set; }
        public ApplictionUser User { get; set; }
        public ICollection<OnlineOrder> Orders { get; set; }
    }
}
