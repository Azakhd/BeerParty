﻿using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeerParty.BL.Dto;
using Microsoft.AspNetCore.Authorization;

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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var meeting = new Meeting
                    {
                        Title = dto.Title,
                        CreatorId = creatorId,
                        MeetingTime = dto.MeetingTime,
                        Location = dto.Location,
                        Description = dto.Description,
                        IsPublic = dto.IsPublic,
                        Category = dto.Category // Устанавливаем категорию встречи
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
       .Include(m => m.Creator) // Включаем создателя встречи
           .ThenInclude(u => u.Profile) // Загружаем профиль создателя
       .Include(m => m.Participants) // Включаем участников
           .ThenInclude(p => p.User) // Загружаем пользователей участников
               .ThenInclude(u => u.Profile) // Загружаем профиль каждого участника
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
                    FirstName = meeting.Creator.Profile?.FirstName, // Проверка на null
                    LastName = meeting.Creator.Profile?.LastName,   // Проверка на null
                    ProfilePictureUrl = meeting.Creator.Profile?.PhotoUrl // Проверка на null
                },
                Participants = meeting.Participants.Select(p => new
                {
                    Id = p.User?.Id,
                    FirstName = p.User?.Profile?.FirstName, // Проверка на null
                    LastName = p.User?.Profile?.LastName,   // Проверка на null
                    ProfilePictureUrl = p.User?.Profile?.PhotoUrl // Проверка на null
                }).ToList()
            };


            return Ok(meetingInfo);
        }



        // Присоединение к публичной встрече
        [HttpPost("{meetingId}/join")]
        [Authorize] // Требуется авторизация
        public async Task<IActionResult> JoinMeeting(long meetingId)
        {
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null)
                return NotFound();

            // Проверяем, является ли встреча публичной
            if (!meeting.IsPublic)
                return Forbid(); // Если встреча не публичная

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = long.Parse(userIdClaim.Value);

            // Проверяем, не является ли пользователь уже участником
            var existingParticipant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

            if (existingParticipant != null)
                return Conflict("Вы уже участвуете в этой встрече."); // Пользователь уже участник

            // Создаем запись о новом участнике
            var newParticipant = new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = userId,
                IsConfirmed = false // Можно сделать подтверждение по вашему желанию
            };

            _context.MeetingParticipants.Add(newParticipant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Вы успешно присоединились к встрече.", participant = newParticipant });
        }


    }
}
