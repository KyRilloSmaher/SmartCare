using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Cart.Responses
{
    public class CartResponseDto
    {
        public Guid CartId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public IEnumerable<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();
    }
}
