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
        // Отправка сообщения конкретному пользователю
        public async Task SendMessageToUser(string receiverConnectionId, string message)
        {
            var sender = Context.User.Identity.Name; // Получаем имя отправителя из контекста

            // Отправляем сообщение пользователю с указанным ConnectionId
            await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", new ChatMessage
            {
                Sender = sender,
                Receiver = receiverConnectionId,
                Message = message,
                Timestamp = DateTime.Now
            });
        }

        // Подключение пользователя
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity.Name;
            await Clients.All.SendAsync("UserConnected", userName);
            await base.OnConnectedAsync();
        }
    }

}
