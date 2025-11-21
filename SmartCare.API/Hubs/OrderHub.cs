using Microsoft.AspNetCore.SignalR;
using Polly;
using System.Text.RegularExpressions;

namespace SmartCare.API.Hubs
{
    public class OrderHub :Hub
    {
        public async Task JoinProductGroup(Guid orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order:{orderId}");
        }

        public async Task LeaveProductGroup(Guid orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order:{orderId}");
        }
    }
}
