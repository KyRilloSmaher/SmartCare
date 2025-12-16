using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCare.API.Hubs
{
    [Authorize]
    public class ProductsHub : Hub
    {
        public override async Task OnConnectedAsync() => await base.OnConnectedAsync();

        public override async Task OnDisconnectedAsync(Exception? exception) => await base.OnDisconnectedAsync(exception);
        public async Task JoinProductGroup(Guid productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"product:{productId}");
        }

        public async Task LeaveProductGroup(Guid productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"product:{productId}");
        }
    }
}
