using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Inventory.Request
{
    public class CreateInventoryRequestDto
    {
        public Guid StoreId { get; set; }
        public Guid ProductId { get; set; }
        public int StockQuantity {  get; set; }
        public int ReservedQuantity {  get; set; }
    }
}
