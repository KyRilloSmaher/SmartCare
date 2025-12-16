using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCare.API.Hubs
{
    [Authorize]
    public class UserNotificationHub :Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their own group (so we can target them by user ID)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
