using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Inventory.Response
{
    public class InventoryUserResponseDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public int AvailableQuantity {  get; set; }
        public string ProductName {  get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

    }
}
