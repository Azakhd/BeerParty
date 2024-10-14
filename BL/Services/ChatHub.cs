using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        // Отправляем сообщение всем подключённым клиентам
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendPrivateMessage(string user, string receiverConnectionId, string message)
    {
        // Отправляем сообщение конкретному пользователю
        await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", user, message);
    }

    public override Task OnConnectedAsync()
    {
        var userName = Context.User?.Identity?.Name;
        if (userName != null)
        {
            // Логика при подключении пользователя
            Console.WriteLine($"{userName} подключился к чату.");
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var userName = Context.User?.Identity?.Name;
        if (userName != null)
        {
            // Логика при отключении пользователя
            Console.WriteLine($"{userName} покинул чат.");
        }
        return base.OnDisconnectedAsync(exception);
    }
}
