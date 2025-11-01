using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Cart.Requests
{
    public class RemoveFromCartRequestDto
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ReservationId { get; set; }
    }
}
