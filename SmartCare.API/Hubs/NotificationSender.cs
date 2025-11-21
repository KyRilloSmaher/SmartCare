using Microsoft.AspNetCore.SignalR;
using SmartCare.API.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Notifications
{
    public class SignalRNotificationSender : INotificationSender
    {
        private readonly IHubContext<UserNotificationHub> _hub;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly IHubContext<ProductsHub> _productHub;
        private readonly IHubContext<PaymentsHub> _paymentHub;
        private readonly IHubContext<CartHub> _cartHub;

        public SignalRNotificationSender(IHubContext<UserNotificationHub> hub, IHubContext<ProductsHub> productHub, IHubContext<PaymentsHub> paymentHub, IHubContext<CartHub> cartHub, IHubContext<OrderHub> orderHub)
        {
            _hub = hub;
            _productHub = productHub;
            _paymentHub = paymentHub;
            _cartHub = cartHub;
            _orderHub = orderHub;
        }

        public Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default)
        {
            // using user identifier from claims mapping (SignalR user identifiers)
            return _hub.Clients.User(userId).SendAsync(method, payload, ct);
        }

        public Task SendToGroupAsync(string groupName, string method, object payload, CancellationToken ct = default)
            => _hub.Clients.Group(groupName).SendAsync(method, payload, ct);

        public async Task SendOrderExpiration(Guid groupName , object payload, CancellationToken ct = default)
        {
            // Push notification to the right client group
            await _orderHub.Clients.Group($"order:{groupName}")
                .SendAsync("OrderExpire", payload);

            Console.WriteLine($"[SignalR] Sent Expiration Alert for Order {groupName} ");
        }
        public async Task SendProductStockStatusChangedAsync(Guid productId, object payload, CancellationToken ct = default)
        {
            // Push notification to the right client group
            await _productHub.Clients.Group($"product:{productId}")
                .SendAsync("ProductStockStatusChanged", payload);

            Console.WriteLine($"[SignalR] Product {productId} stock updated ");
        }
        public async Task SendPaymentStatusChangedAsync(string clientId, object payload, CancellationToken ct = default)
        {
            // Push notification to the right client group
            await _paymentHub.Clients.Group($"client:{clientId}")
                .SendAsync("PaymentStatusChanged", payload);

            Console.WriteLine($"[SignalR] Sent PaymentStatusChanged for Client {clientId} ");
        }

        public async Task SendCartUpdatedAsync(Guid cartId, object payload, CancellationToken ct = default)
        {
            // Push notification to the right client group
            await _cartHub.Clients.Group($"cart:{cartId}")
                .SendAsync("ReservationExpired", payload);

            Console.WriteLine($"[SignalR] Sent CartUpdated for cartId {cartId} ");
        }

    }
}
