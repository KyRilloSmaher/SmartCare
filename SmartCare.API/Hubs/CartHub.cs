using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCare.API.Hubs
{
    [Authorize]
    public class CartHub : Hub
    {
        public async Task JoinProductGroup(Guid CartId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"cart:{CartId}");
        }

        public async Task LeaveProductGroup(Guid CartId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cart:{CartId}");
        }
    }
}
