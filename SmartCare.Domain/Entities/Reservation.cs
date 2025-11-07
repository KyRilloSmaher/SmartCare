using SmartCare.Domain.Enums;
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
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public ReservationStatus Status { get; set; }
        public CartItem CartItem { get; set; }

    }
}
