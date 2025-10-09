using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid CartItemId { get; set; }
        public int QuantityReserved { get; set; }
        public int ReservedAt { get; set; }
        public int ExpiredAt { get; set; }
        public CartItem CartItem { get; set; }

    }
}
