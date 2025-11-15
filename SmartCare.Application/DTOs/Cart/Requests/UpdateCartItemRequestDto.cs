using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Cart.Requests
{
    public class UpdateCartItemRequestDto
    {
        public Guid CartId { get; set; }
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public int NewQuantity { get; set; }
    }
}
