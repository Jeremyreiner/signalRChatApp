using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using signalR.Services;

namespace signalR.Hubs
{
    public class ChatHubs : Hub
    {
        private readonly ChatService _chatService;

        public ChatHubs(ChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
            
            await Clients.Caller.SendAsync("UserConnected");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
