using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartCare.API.Hubs;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.Notifications
{
    [Authorize]
    public class SignalRNotificationSender : INotificationSender
    {
        private readonly IHubContext<UserNotificationHub> _userHub;
        private readonly IHubContext<ProductsHub> _productHub;
        private readonly IHubContext<PharmacistHub> _pharmacistHub;

        public SignalRNotificationSender(IHubContext<UserNotificationHub> userHub, IHubContext<ProductsHub> productHub, IHubContext<PharmacistHub> pharmacistHub)
        {
            _userHub = userHub;
            _productHub = productHub;
            _pharmacistHub = pharmacistHub;
        }

        //public SignalRNotificationSender(
        //    IHubContext<UserNotificationHub> userHub,
        //    IHubContext<ProductsHub> productHub)
        //{
        //    _userHub = userHub;
        //    _productHub = productHub;
        //}

        // ===== BASIC METHODS =====
        public Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default)
            => _userHub.Clients.User(userId).SendAsync(method, payload, ct);

        public Task SendToUserGroupAsync(string userId, string method, object payload, CancellationToken ct = default)
            => _userHub.Clients.Group($"user:{userId}").SendAsync(method, payload, ct);

        // =============================
        //         USER NOTIFICATIONS
        // =============================

        // Order Expired
        public async Task SendOrderExpiration(string userId, Guid orderId, object payload, CancellationToken ct = default)
        {
            await _userHub.Clients.Group($"user:{userId}")
                .SendAsync("OrderExpire", payload, ct);

            Console.WriteLine($"[SignalR] OrderExpire -> User {userId}, Order {orderId}");
        }

        // Payment Status Change
        public async Task SendPaymentStatusChangedAsync(string userId, object payload, CancellationToken ct = default)
        {
            await _userHub.Clients.Group($"user:{userId}")
                .SendAsync("PaymentStatusChanged", payload, ct);

            Console.WriteLine($"[SignalR] PaymentStatusChanged -> User {userId}");
        }

        // Reservation Expired / Cart Updated
        public async Task SendCartUpdatedAsync(string userId, Guid cartId, object payload, CancellationToken ct = default)
        {
            await _userHub.Clients.Group($"user:{userId}")
                .SendAsync("ReservationExpired", payload, ct);

            Console.WriteLine($"[SignalR] ReservationExpired -> User {userId}, Cart {cartId}");
        }

        // =============================
        //    PRODUCT NOTIFICATIONS
        // =============================
        public async Task SendProductStockStatusChangedAsync(Guid productId, object payload, CancellationToken ct = default)
        {
            await _productHub.Clients.Group($"product:{productId}")
                .SendAsync("ProductStockStatusChanged", payload, ct);

            Console.WriteLine($"[SignalR] ProductStockStatusChanged -> Product {productId}");
        }


        // ==============================
        //      ORDER NOTIFICATIONS
        // ===============================


        public async Task SendNewOnlineOrderToStoreAsync(
            Guid storeId, object payload, CancellationToken ct = default)
        {
            await _pharmacistHub.Clients
                .Group($"store:{storeId}")
                .SendAsync("NewOnlineOrder", payload, ct);

            Console.WriteLine($"[SignalR] NewOnlineOrder -> Store {storeId}");
        }

        public async Task SendNewPickUpOrderToStoreAsync(
              Guid storeId, object payload, CancellationToken ct = default)
        {
            await _pharmacistHub.Clients
                .Group($"store:{storeId}")
                .SendAsync("NewPickUpOrder", payload, ct); 
            Console.WriteLine($"[SignalR] NewPickUpOrder -> Store {storeId}");
        }
    }
}
