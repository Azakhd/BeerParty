using BeerParty.BL.Dto;

using BeerParty.Data;
using BeerParty.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeerParty.Web.Controllers
{
    public class MessagesController : BaseController
    {
        private readonly ApplicationContext _context;

        public MessagesController(ApplicationContext context)
        {
            _context = context;
        }

        // Метод для отправки сообщения
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto model)
        {
            var userId =  User.FindFirst(ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var sender = await _context.Users.SingleOrDefaultAsync(u => u.Name == userId);
            if (sender == null)
            {
                return NotFound("Пользователь не найден");
            }

            var receiver = await _context.Users.FindAsync(model.ReceiverId);
            if (receiver == null)
            {
                return NotFound("Получатель не найден");
            }

            var message = new MessageEntity
            {
                SenderId = sender.Id,
                RecipientId = receiver.Id,
                Content = model.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok("Сообщение отправлено");
        }
        // Получение сообщений между пользователями
        [HttpGet("conversation/{friendId}")]
        public async Task<IActionResult> GetConversation(long friendId)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var currentUser = await _context.Users.SingleOrDefaultAsync(u => u.Name == userName);
            if (currentUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.RecipientId == friendId) ||
                             (m.SenderId == friendId && m.RecipientId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new { m.Sender!.Name, m.Content, m.SentAt })
                .ToListAsync();

            return Ok(messages);
        }
    }
}