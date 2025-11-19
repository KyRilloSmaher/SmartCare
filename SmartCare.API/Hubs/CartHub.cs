using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCare.API.Hubs
{
    [Authorize]
    public class CartHub : Hub
    {
        public async Task JoinCartGroup(Guid CartId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"cart:{CartId}");
        }

        public async Task LeaveCartGroup(Guid CartId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cart:{CartId}");
        }
    }
}
