using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Events
{
    public class ProductStockStatusChangedEvent
    {
        public Guid ProductId { get; set; }
        public bool isAvailable { get; set; }
        public ProductStockStatusChangedEvent(Guid id , bool status) {
             ProductId = id;
            isAvailable = status;                                                   
        }
    }
}
