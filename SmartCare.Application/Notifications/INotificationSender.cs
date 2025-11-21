using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Notifications
{
    public interface INotificationSender
    {
        Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default);
        Task SendToGroupAsync(string groupName, string method, object payload, CancellationToken ct = default);
        Task SendOrderExpiration(Guid groupName,object payload, CancellationToken ct = default);
        Task SendProductStockStatusChangedAsync(Guid productId, object payload, CancellationToken ct = default);
        Task SendPaymentStatusChangedAsync(string clientId, object payload, CancellationToken ct = default);
        Task SendCartUpdatedAsync(Guid cartId, object payload, CancellationToken ct = default);
        
    }
}
