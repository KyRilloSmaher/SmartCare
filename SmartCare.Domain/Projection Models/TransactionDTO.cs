using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class TransactionDTO
    {
        public Guid OrderId { get; set; }
        public List<Guid> productIds { get; set; }
    }
}
