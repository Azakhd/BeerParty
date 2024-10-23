using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeerParty.BL.Dto;
using Microsoft.AspNetCore.Authorization;
using System;
using BeerParty.Data.Enums;

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
        public async Task<IActionResult> CreateMeeting([FromForm] CreateMeetingDto dto)
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
                        ParticipantLimit = dto.ParticipantLimit,
                        Latitude = dto.Latitude,
                        Longitude = dto.Longitude// Устанавливаем лимит участников
                    };

                    // Обработка загрузки изображения
                    if (dto.Photo != null && dto.Photo.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "BeerParty.Data", "Uploads", "MeetingPictures");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await dto.Photo.CopyToAsync(stream);
                        }

                        meeting.PhotoUrl = $"{Request.Scheme}://{Request.Host}/uploads/MeetingPictures/{fileName}";
                    }
                    else
                    {
                        // Устанавливаем значение по умолчанию или оставляем null
                        meeting.PhotoUrl = "default_photo_url"; // Убедитесь, что это значение допустимо в вашей базе данных
                    }

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
        [HttpGet("FindMeetings")]
        public async Task<IActionResult> FindMeetings(double latitude, double longitude, double radius)
        {
            const double EarthRadius = 6371; // Радиус Земли в километрах

            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // Если не удается получить ID пользователя, возвращаем 401

            var userId = long.Parse(userIdClaim.Value);

            // Получаем все встречи, доступные текущему пользователю
            var meetings = await _context.Meetings
                .Where(m => m.IsPublic || m.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            // Фильтруем встречи по геолокации
            var filteredMeetings = meetings
                .Where(m =>
                {
                    var dLat = (m.Latitude - latitude) * (Math.PI / 180);
                    var dLon = (m.Longitude - longitude) * (Math.PI / 180);
                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                            Math.Cos(latitude * (Math.PI / 180)) * Math.Cos(m.Latitude * (Math.PI / 180)) *
                            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var distance = EarthRadius * c;

                    return distance <= radius; // Возвращаем встречи, находящиеся в радиусе
                })
                .ToList();

            return Ok(filteredMeetings);
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
            try
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

                // Проверяем, является ли пользователь создателем встречи или администратором
                bool isCreator = meeting.CreatorId == userId;
                bool isAdmin = User.IsInRole("Admin"); // Проверяем, является ли пользователь администратором

                // Проверяем, является ли встреча публичной, пользователь создатель или администратор
                bool isUserParticipant = meeting.IsPublic || isCreator || isAdmin || meeting.Participants.Any(p => p.UserId == userId);

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
                    meeting.PhotoUrl, // Добавляем фото встречи в ответ
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
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем 500
                // Logger.LogError(ex, "Error retrieving meeting with ID {MeetingId}", id);
                return StatusCode(500, "Произошла ошибка при получении встречи.");
            }
        }

        [HttpPut("UpdateMeeting/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMeeting(long id, [FromForm] MeetingUpdateDto meetingUpdateDto)
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

            // Обновляем поля встречи
            meeting.Title = meetingUpdateDto.Title ?? meeting.Title;
            meeting.MeetingTime = meetingUpdateDto.MeetingTime != default ? meetingUpdateDto.MeetingTime : meeting.MeetingTime;
            meeting.Location = meetingUpdateDto.Location ?? meeting.Location;
            meeting.Description = meetingUpdateDto.Description ?? meeting.Description;
            meeting.IsPublic = meetingUpdateDto.IsPublic;

            if (meetingUpdateDto.ParticipantLimit.HasValue)
            {
                meeting.ParticipantLimit = meetingUpdateDto.ParticipantLimit.Value; // Обновляем лимит участников
            }

            // Обработка загрузки фотографии для встречи
            if (meetingUpdateDto.Photo != null && meetingUpdateDto.Photo.Length > 0)
            {
                try
                {
                    // Создание уникального имени файла
                    var fileName = $"{id}_{Guid.NewGuid()}{Path.GetExtension(meetingUpdateDto.Photo.FileName)}";

                    // Путь к папке Uploads в проекте BeerParty.Data
                    var meetingFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "BeerParty.Data", "Uploads", "MeetingPictures");
                    var filePath = Path.Combine(meetingFolder, fileName);

                    // Создать папку, если она не существует
                    if (!Directory.Exists(meetingFolder))
                    {
                        Directory.CreateDirectory(meetingFolder);
                    }

                    // Удаление старого фото, если оно существует
                    if (!string.IsNullOrEmpty(meeting.PhotoUrl))
                    {
                        var oldPhotoPath = Path.Combine(meetingFolder, Path.GetFileName(meeting.PhotoUrl));
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    // Сохранение нового файла
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await meetingUpdateDto.Photo.CopyToAsync(stream);
                    }

                    // URL для доступа к загруженному файлу
                    meeting.PhotoUrl = $"{Request.Scheme}://{Request.Host}/uploads/MeetingPictures/{fileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Ошибка при загрузке фото: {ex.Message}");
                }
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
        [Authorize]
        [HttpPost("like")]
        public async Task<IActionResult> ToggleLike(long meetingId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var like = await _context.Likes
                .SingleOrDefaultAsync(l => l.UserId == userId && l.MeetingId == meetingId);

            if (like != null)
            {
                // Удаляем лайк
                _context.Likes.Remove(like);
            }
            else
            {
                // Добавляем лайк
                var newLike = new Like { UserId = userId.Value, MeetingId = meetingId };
                await _context.Likes.AddAsync(newLike);
            }

            await _context.SaveChangesAsync();
            return Ok("Изменения сохранены");
        }

        [HttpGet("recommended")] // Это делает его API-эндпоинтом
        public async Task<IActionResult> GetRecommendedMeetings(int page = 0, int pageSize = 10)
        {
            var meetings = await GetRecommendedMeetingsAsync(pageSize);
            return Ok(meetings);
        }

        // Если вы хотите, чтобы этот метод оставался приватным, переименуйте его, например, так:
        private async Task<List<Meeting>> GetRecommendedMeetingsAsync(int count)
        {
            // Получаем встречи и сортируем их по количеству лайков и отзывов
            var meetings = await _context.Meetings
                .Select(m => new
                {
                    Meeting = m,
                    LikesCount = m.Likes.Count(),
                    ReviewsCount = m.Reviews.Count()
                })
                .OrderByDescending(m => m.LikesCount + m.ReviewsCount) // Сортировка по количеству лайков и отзывов
                .Take(count) // Ограничиваем количество возвращаемых встреч
                .ToListAsync(); // Выполняем запрос асинхронно

            // Преобразуем результат в список встреч
            return meetings.Select(m => m.Meeting).ToList(); // Здесь возвращаем Meeting
        }


        [HttpGet("personal-recommendations")]
        public async Task<IActionResult> GetPersonalRecommendations(int page = 0, int pageSize = 10)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Получаем встречи, которые пользователь отметил как "нравится"
            var userLikes = await _context.Likes
                .Where(l => l.UserId == userId)
                .Select(l => l.MeetingId)
                .ToListAsync();

            // Получаем рекомендации на основе встреч, которые пользователь лайкнул
            var recommendedMeetings = await _context.MeetingReviews
                .Where(r => userLikes.Contains(r.MeetingId) && r.Rating >= 4) // Учитываем только встречи с высоким рейтингом
                .Select(r => r.Meeting)
                .Distinct()
                .Include(m => m.Creator) // Загрузка создателя встречи
                .Include(m => m.Participants) // Загрузка участников встречи
                .Skip(page * pageSize) // Пропускаем встречи для пагинации
                .Take(pageSize) // Ограничиваем количество возвращаемых встреч
                .ToListAsync();

            return Ok(recommendedMeetings);
        }

    }
}
