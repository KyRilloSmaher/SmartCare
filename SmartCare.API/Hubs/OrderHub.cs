using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Polly;
using System.Text.RegularExpressions;

namespace SmartCare.API.Hubs
{
    [Authorize]
    public class OrderHub :Hub
    {
        public override async Task OnConnectedAsync() => await base.OnConnectedAsync();

        public override async Task OnDisconnectedAsync(Exception? exception) => await base.OnDisconnectedAsync(exception);
    }
}
