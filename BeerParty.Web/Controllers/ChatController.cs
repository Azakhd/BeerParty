using BeerParty.BL.Services;
using BeerParty.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BeerParty.Web.Controllers
{
    public class ChatController : BaseController
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            // Отправляем сообщение всем клиентам через хаб
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Message);
            return Ok(new { Status = "Message Sent" });
        }
    }

}
