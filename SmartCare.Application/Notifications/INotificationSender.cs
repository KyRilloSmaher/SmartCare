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
        Task SendToUserGroupAsync(string groupName, string method, object payload, CancellationToken ct = default);
        Task SendOrderExpiration(string userId ,Guid groupName,object payload, CancellationToken ct = default);
        Task SendProductStockStatusChangedAsync(Guid productId, object payload, CancellationToken ct = default);
        Task SendPaymentStatusChangedAsync(string clientId, object payload, CancellationToken ct = default);
        Task SendCartUpdatedAsync(string userId ,Guid cartId, object payload, CancellationToken ct = default);
        Task SendNewOnlineOrderToStoreAsync(Guid storeId, object payload, CancellationToken ct = default);
        Task SendNewPickUpOrderToStoreAsync(Guid storeId,object payload,CancellationToken ct = default);
    }
}
