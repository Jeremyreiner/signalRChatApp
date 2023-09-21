using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using signalR.Dtos;
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
            
            //Calls angular method from services/chat.service.ts file
            await Clients.Caller.SendAsync("UserConnected");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");

            var user = _chatService.GetUserByConnectionId(Context.ConnectionId);

            _chatService.RemoveUserFromList(user);

            await DisplayOnlineUsers();

            await base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        /// This method will be invoked directly from services/chat.service.ts file'
        /// method name NEEDs to match in angular service file
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task AddUserConnectionId(string name)
        {
            _chatService.AddUserConnectionId(name, Context.ConnectionId);

            await DisplayOnlineUsers();
        }

        public async Task CreatePrivateChat(MessageDto message)
        {
            var privateGroupName = GetPrivateGroupName(message.From, message.To);

            await Groups.AddToGroupAsync(Context.ConnectionId, privateGroupName);

            var toConnectionId = _chatService.GetConnectionIdByUser(message.To);

            await Groups.AddToGroupAsync(toConnectionId, privateGroupName);

            //opens private chat box for the other end user
            await Clients.Client(toConnectionId).SendAsync("OpenPrivateChat", message);
        }

        public async Task ReceivePrivateMessage(MessageDto message)
        {
            var privateGroupName = GetPrivateGroupName(message.From, message.To);

            await Clients.Group(privateGroupName).SendAsync("NewPrivateMessage", message);
        }

        public async Task RemovePrivateChat(string from, string to)
        {
            var privateGroupName = GetPrivateGroupName(from, to);

            await Clients.Group(privateGroupName).SendAsync("ClosePrivateChat");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, privateGroupName);

            var toConnectionId = _chatService.GetConnectionIdByUser(to);

            await Groups.RemoveFromGroupAsync(toConnectionId, privateGroupName);
        }

        public async Task ReceiveMessage(MessageDto message)
        {
            await Clients.Groups("Come2Chat").SendAsync("NewMessage", message);
        }

        private async Task DisplayOnlineUsers()
        {
            var onlineUsers = _chatService.GetOnlineUsers();

            await Clients.Groups("Come2Chat").SendAsync("OnlineUsers", onlineUsers);
        }

        private string GetPrivateGroupName(string from, string to)
        {
            var stringCompare = string.Compare(from, to) < 0;

            return stringCompare 
                ? $"{from}-{to}" 
                : $"{to}-{from}";
        }
    }
}
