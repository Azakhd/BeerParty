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
            if (message == null || string.IsNullOrEmpty(message.Receiver))
            {
                return BadRequest("Message or receiver is null.");
            }

            await _hubContext.Clients.User(message.Receiver).SendAsync("ReceiveMessage", message.Sender, message.Message);
            return Ok();
        }
    }

}
