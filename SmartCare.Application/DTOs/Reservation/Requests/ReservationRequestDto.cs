using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Reservation.Requests
{
    public class ReservationRequestDto
    {
        public Guid CartItemId { get; set; }
        public int QuantityReserved { get; set; }
        public DateTime ReservedAt { get; set; }
    }
}
