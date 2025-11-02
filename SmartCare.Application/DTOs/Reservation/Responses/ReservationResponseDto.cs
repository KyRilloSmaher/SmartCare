using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Reservation.Responses
{
    public class ReservationResponseDto
    {
        public Guid ReservationId { get; set; }
        public int QuantityReserved { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
