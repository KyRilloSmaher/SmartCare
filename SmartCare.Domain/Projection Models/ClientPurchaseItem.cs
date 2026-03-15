using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    // DTO for client purchase history
    public class ClientPurchaseItem
    {
        public Guid ProductId { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
