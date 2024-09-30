using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string sender, string receiver, string message)
        {
            await Clients.User(receiver).SendAsync("ReceiveMessage", sender, message);
        }
    }
}
