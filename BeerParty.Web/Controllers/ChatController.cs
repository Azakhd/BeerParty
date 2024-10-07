using BeerParty.BL.Services;
using BeerParty.Data;
using BeerParty.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BeerParty.Web.Controllers
{
    public class ChatController : BaseController
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ApplicationContext _context;

        public ChatController(IHubContext<ChatHub> hubContext, ApplicationContext context)
        {
            _hubContext = hubContext;
            _context = context; // Инициализируем контекст
        }   

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            // Отправляем сообщение конкретному пользователю через хаб
            await _hubContext.Clients.User(message.Receiver).SendAsync("ReceiveMessage", message.Sender, message.Message);
            return Ok(new { Status = "Message Sent" });
        }
      
    }


}
