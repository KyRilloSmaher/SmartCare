using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCare.API.Hubs
{
    [Authorize(Roles = "PHARMACIST")]
    public class PharmacistHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            var storeId = Context.User?.FindFirst("StoreId")?.Value;
            if (!string.IsNullOrEmpty(storeId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"store:{storeId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

            var storeId = Context.User?.FindFirst("StoreId")?.Value;
            if (!string.IsNullOrEmpty(storeId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store:{storeId}");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinStoreGroup(string storeId)
        {
           
            var claimStoreId = Context.User?.FindFirst("StoreId")?.Value;
            if (claimStoreId != storeId)
            {
                Console.WriteLine($"[SignalR] Unauthorized attempt by {Context.UserIdentifier}");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, $"store:{storeId}");
        }

        public async Task LeaveStoreGroup(string storeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store:{storeId}");
            Console.WriteLine($"[SignalR] Pharmacist left store:{storeId}");
        }
    }
}