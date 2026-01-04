

using SmartCare.Domain.Enums;

namespace SmartCare.Application.DTOs.Orders.Requests
{
    public class UpdateOrderRequestDto
    {
        public Guid OrderId { get; set; }
        public Guid CartId { get; set; }
        public OrderType UpdatedOrderType { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? ShippingAddressId { get; set; }
    }
}
