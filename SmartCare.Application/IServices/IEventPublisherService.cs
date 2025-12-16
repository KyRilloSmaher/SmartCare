using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{

        public interface IEventPublisherService
        {
        Task PublishReservationExpired(Guid cartId, Guid productId, int quantity, string userid);
        Task PublishPaymentStatusChanged(Guid orderId, string clientId, string status, string message);
        Task PublishProductStockStatusChanged(Guid productId, bool isAvailable);
        Task PublishOrderExpirationNotification(string userid ,Guid orderId);
    }

    
}
