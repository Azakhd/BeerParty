using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeerParty.BL.Dto;

namespace BeerParty.Web.Controllers
{
    public class MeetingsController : BaseController
    {
        private readonly ApplicationContext _context;

        public MeetingsController(ApplicationContext context)
        {
            _context = context;
        }

        // Создание новой встречи
        [HttpPost("CreateMeeting")]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
        {
            if (dto == null)
                return BadRequest();

            var creatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (creatorIdClaim == null)
                return Unauthorized(); // Если токен недействителен

            var creatorId = int.Parse(creatorIdClaim.Value);
            var participants = new List<MeetingParticipant>();

            foreach (var participantId in dto.ParticipantIds)
            {
                // Проверяем, существует ли пользователь с данным participantId
                var userExists = await _context.Users.AnyAsync(u => u.Id == participantId);

                if (userExists)
                {
                    participants.Add(new MeetingParticipant
                    {
                        UserId = participantId,
                        IsConfirmed = false // Или true, в зависимости от вашей логики
                    });
                }
                else
                {
                    return BadRequest($"Пользователь с ID {participantId} не найден.");
                }
            }

            // Если есть хотя бы один участник, создаем встречу
            if (participants.Any())
            {
                var meeting = new Meeting
                {
                    Title = dto.Title,
                    CreatorId = creatorId,
                    MeetingTime = dto.MeetingTime,
                    Location = dto.Location,
                    Description = dto.Description
                };

                _context.Meetings.Add(meeting);
                await _context.SaveChangesAsync(); // Сохраняем встречу, чтобы получить ее Id

                // Теперь добавляем участников
                foreach (var participant in participants)
                {
                    participant.MeetingId = meeting.Id; // Присваиваем MeetingId
                    _context.MeetingParticipants.Add(participant);
                }

                await _context.SaveChangesAsync(); // Сохраняем участников
                return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, meeting);
            }
            else
            {
                return BadRequest("Не указаны участники для встречи.");
            }
        }


            // Подтверждение участия в встрече
            [HttpPost("{meetingId}/participants")]
        public async Task<IActionResult> ConfirmParticipation(long meetingId, [FromBody] long userId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null)
                return NotFound();

            var participant = new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = userId,
                IsConfirmed = true
            };

            _context.MeetingParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(participant);
        }

        // Получение информации о встрече
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeeting(long id)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
                return NotFound();

            return Ok(meeting);
        }
    }
}
