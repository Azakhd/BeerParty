using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeerParty.BL.Dto;
using Microsoft.AspNetCore.Authorization;
using System;

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
                return Unauthorized();

            var creatorId = int.Parse(creatorIdClaim.Value);
            var participants = new List<MeetingParticipant>();

            foreach (var participantId in dto.ParticipantIds)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == participantId);
                if (userExists)
                {
                    participants.Add(new MeetingParticipant
                    {
                        UserId = participantId,
                        IsConfirmed = false
                    });
                }
                else
                {
                    return BadRequest($"Пользователь с ID {participantId} не найден.");
                }
            }

            if (dto.ParticipantLimit.HasValue && dto.ParticipantLimit.Value < participants.Count)
            {
                return BadRequest($"Количество участников превышает лимит в {dto.ParticipantLimit.Value}.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    long? coAuthorId = dto.CoAuthorId.HasValue && dto.CoAuthorId.Value != 0 ? dto.CoAuthorId.Value : (long?)null;

                    var meeting = new Meeting
                    {
                        Title = dto.Title,
                        CreatorId = creatorId,
                        MeetingTime = dto.MeetingTime,
                        Location = dto.Location,
                        Description = dto.Description,
                        IsPublic = dto.IsPublic,
                        CoAuthorId = coAuthorId,
                        Category = dto.Category,
                        ParticipantLimit = dto.ParticipantLimit // Устанавливаем лимит участников
                    };

                    _context.Meetings.Add(meeting);
                    await _context.SaveChangesAsync();

                    foreach (var participant in participants)
                    {
                        participant.MeetingId = meeting.Id;
                        _context.MeetingParticipants.Add(participant);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var createdMeeting = await _context.Meetings
                        .Include(m => m.Creator)
                        .Include(m => m.Participants)
                        .FirstOrDefaultAsync(m => m.Id == meeting.Id);

                    return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, createdMeeting);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Произошла ошибка при создании встречи.");
                }
            }
        }


        // Подтверждение участия в встрече
        [HttpPost("{meetingId}/participants/confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmParticipation(long meetingId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Пользователь не авторизован.");

            long userId = long.Parse(userIdClaim.Value);

            // Находим встречу по ID
            var meeting = await _context.Meetings
                .Include(m => m.Participants) // Загружаем участников встречи
                .ThenInclude(p => p.User) // Загружаем пользователей, связанных с участниками
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
                return NotFound("Встреча не найдена.");

            // Проверяем, является ли пользователь участником встречи
            var participant = await _context.MeetingParticipants
                .Include(p => p.User) // Загружаем пользователя
                .FirstOrDefaultAsync(p => p.MeetingId == meetingId && p.UserId == userId);

            if (participant == null)
                return BadRequest("Вы не являетесь участником этой встречи.");

            // Проверяем, уже подтверждено ли участие
            if (participant.IsConfirmed)
                return BadRequest("Ваше участие уже подтверждено.");

            // Подтверждаем участие
            participant.IsConfirmed = true;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Ваше участие подтверждено.", Participant = participant });
        }



        // Получение информации о встрече

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeeting(long id)
        {
            var meeting = await _context.Meetings
       .Include(m => m.Creator)
           .ThenInclude(u => u.Profile)
       .Include(m => m.Participants)
           .ThenInclude(p => p.User)
               .ThenInclude(u => u.Profile)
       .SingleOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
                return NotFound();

            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // Если не удается получить ID пользователя, возвращаем 401

            var userId = long.Parse(userIdClaim.Value);

            // Проверяем, является ли встреча публичной или пользователь является участником
            bool isUserParticipant = meeting.IsPublic || meeting.Participants.Any(p => p.UserId == userId);

            if (!isUserParticipant)
            {
                return Forbid(); // Запрещаем доступ, если встреча частная и пользователь не участник
            }

            // Формируем ответ с информацией о встрече
            var meetingInfo = new
            {
                meeting.Id,
                meeting.Title,
                meeting.MeetingTime,
                meeting.Location,
                meeting.Description,
                meeting.IsPublic,
                meeting.Category,
                Creator = new
                {
                    Id = meeting.Creator.Id,
                    FirstName = meeting.Creator.Profile?.FirstName,
                    LastName = meeting.Creator.Profile?.LastName,
                    ProfilePictureUrl = meeting.Creator.Profile?.PhotoUrl
                },
                CoAuthor = meeting.CoAuthor != null ? new
                {
                    Id = meeting.CoAuthor.Id,
                    FirstName = meeting.CoAuthor.Profile?.FirstName,
                    LastName = meeting.CoAuthor.Profile?.LastName,
                    ProfilePictureUrl = meeting.CoAuthor.Profile?.PhotoUrl
                } : null, // Если соавтор не задан, возвращаем null
                Participants = meeting.Participants.Select(p => new
                {
                    Id = p.User?.Id,
                    FirstName = p.User?.Profile?.FirstName,
                    LastName = p.User?.Profile?.LastName,
                    ProfilePictureUrl = p.User?.Profile?.PhotoUrl
                }).ToList()
            };


            return Ok(meetingInfo);
        }

        [HttpPut("UpdateMeeting{id}")]
        public async Task<IActionResult> UpdateMeeting(long id, MeetingUpdateDto meetingUpdateDto)
        {
            // Находим встречу по ID
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim.Value);
            bool isCreator = meeting.CreatorId == userId;
            bool isCoAuthor = meeting.CoAuthorId == userId;
            bool isAdmin = User.IsInRole("Admin");

            if (!isCreator && !isCoAuthor && !isAdmin)
            {
                return Forbid();
            }

            meeting.Title = meetingUpdateDto.Title ?? meeting.Title;
            meeting.MeetingTime = meetingUpdateDto.MeetingTime != default ? meetingUpdateDto.MeetingTime : meeting.MeetingTime;
            meeting.Location = meetingUpdateDto.Location ?? meeting.Location;
            meeting.Description = meetingUpdateDto.Description ?? meeting.Description;
            meeting.IsPublic = meetingUpdateDto.IsPublic;

            if (meetingUpdateDto.ParticipantLimit.HasValue)
            {
                meeting.ParticipantLimit = meetingUpdateDto.ParticipantLimit.Value; // Обновляем лимит участников
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Присоединение к публичной встрече
        [HttpPost("{meetingId}/join")]
        public async Task<IActionResult> JoinMeeting(long meetingId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null)
                return NotFound();

            if (!meeting.IsPublic)
                return Forbid();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = long.Parse(userIdClaim.Value);

            var existingParticipant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

            if (existingParticipant != null)
                return Conflict("Вы уже участвуете в этой встрече.");

            // Проверяем лимит участников
            var participantCount = await _context.MeetingParticipants.CountAsync(mp => mp.MeetingId == meetingId);
            if (meeting.ParticipantLimit.HasValue && participantCount >= meeting.ParticipantLimit.Value)
            {
                return BadRequest("Лимит участников для этой встречи достигнут.");
            }

            var newParticipant = new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = userId,
                IsConfirmed = false
            };

            _context.MeetingParticipants.Add(newParticipant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Вы успешно присоединились к встрече.", participant = newParticipant });
        }



    }
}
