using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Services
{
    using BeerParty.Data.Entities;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity.Name;
            _connections[userName] = Context.ConnectionId; 
            await Clients.All.SendAsync("UserConnected", userName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity.Name;
            _connections.Remove(userName); // Удаляем Connection ID при отключении
            await Clients.All.SendAsync("UserDisconnected", userName);
        }

        public async Task SendMessageToUser(string receiverUserName, string message)
        {
            if (_connections.TryGetValue(receiverUserName, out var receiverConnectionId))
            {
                var sender = Context.User.Identity.Name;
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", new ChatMessage
                {
                    Sender = sender,
                    Receiver = receiverUserName,
                    Message = message,
                    Timestamp = DateTime.Now
                });
            }
            else
            {
                throw new HubException("Receiver is not connected.");
            }
        }
    }

}
