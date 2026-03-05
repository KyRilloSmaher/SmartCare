using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class PickUpOrder : Order
    {
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
        public string? PickupCodeHash { get; set; }

        public PickUpOrder() { }
        public PickUpOrder(string clientId, decimal total, Guid storeId)
        {
            OrderType = Enums.OrderType.InStore;
            ClientId = clientId;
            TotalPrice = total;
            StoreId = storeId;
        }
        public static PickUpOrder Create(string clientId, decimal total , Guid StoreId)
        {
            return new PickUpOrder(clientId , total , StoreId);
        }

        public void AddPickUpCode(string hashCode)
        {
            PickupCodeHash = hashCode;
        }
    }
}
