using BeerParty.Data.Entities;
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
        public async Task<IActionResult> CreateMeeting([FromForm] CreateMeetingDto dto)
        {
            if (dto == null)
                return BadRequest("Данные встречи не могут быть пустыми.");

            var creatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (creatorIdClaim == null)
                return Unauthorized();

            var creatorId = long.Parse(creatorIdClaim.Value);
            var participants = new List<MeetingParticipant>();

            // Проверяем существование пользователей
            foreach (var participantId in dto.ParticipantIds)
            {
                if (await _context.Users.AnyAsync(u => u.Id == participantId))
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

            // Проверяем лимит участников
            if (dto.ParticipantLimit.HasValue && dto.ParticipantLimit.Value < participants.Count)
            {
                return BadRequest($"Количество участников превышает лимит в {dto.ParticipantLimit.Value}.");
            }

            // Проверяем широту и долготу
            if (!double.TryParse(dto.Latitude.ToString(), out double latitude) ||
                !double.TryParse(dto.Longitude.ToString(), out double longitude))
            {
                return BadRequest("Неверный формат широты или долготы.");
            }

            // Проверка допустимых значений для широты и долготы
            if (latitude < -90 || latitude > 90)
            {
                return BadRequest("Широта должна быть в диапазоне от -90 до 90 градусов.");
            }
            if (longitude < -180 || longitude > 180)
            {
                return BadRequest("Долгота должна быть в диапазоне от -180 до 180 градусов.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    long? coAuthorId = dto.CoAuthorId > 0 ? dto.CoAuthorId : (long?)null;

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
                        Latitude = latitude,
                        Longitude = longitude,
                        // Устанавливаем значение радиуса
                        Radius = dto.Radius
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

                    // Сохранение участников встречи
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
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Произошла ошибка при создании встречи: {ex.Message}");
                }
            }
        }

        [HttpGet("GetMeetingsWithLocation")]
        public async Task<IActionResult> GetMeetingsWithLocation()
        {
            var meetings = await _context.Meetings
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Latitude,
                    m.Longitude // Убедитесь, что у встреч есть координаты
                })
                .ToListAsync();

            return Ok(meetings);
        }

        [HttpGet("FindMeetings")]
        public async Task<IActionResult> FindMeetings(double latitude, double longitude, double radius)
        {
            const double EarthRadius = 6371; // Радиус Земли в километрах

            // Получаем ID пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = long.Parse(userIdClaim.Value);

            var meetings = await _context.Meetings
                .Where(m => m.IsPublic || m.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

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

                    return distance <= radius;
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

            // Обновляем поля встречи с использованием тернарного оператора
            meeting.Title = !string.IsNullOrEmpty(meetingUpdateDto.Title) ? meetingUpdateDto.Title : meeting.Title;
            meeting.MeetingTime = meetingUpdateDto.MeetingTime != default ? meetingUpdateDto.MeetingTime : meeting.MeetingTime;
            meeting.Location = !string.IsNullOrEmpty(meetingUpdateDto.Location) ? meetingUpdateDto.Location : meeting.Location;
            meeting.Description = !string.IsNullOrEmpty(meetingUpdateDto.Description) ? meetingUpdateDto.Description : meeting.Description;
            meeting.IsPublic = meetingUpdateDto.IsPublic; // Обновляем булевое значение напрямую

            meeting.ParticipantLimit = meetingUpdateDto.ParticipantLimit.HasValue ? meetingUpdateDto.ParticipantLimit.Value : meeting.ParticipantLimit;

            // Обновление широты, долготы и радиуса
            meeting.Latitude = meetingUpdateDto.Latitude.HasValue ? meetingUpdateDto.Latitude.Value : meeting.Latitude;
            meeting.Longitude = meetingUpdateDto.Longitude.HasValue ? meetingUpdateDto.Longitude.Value : meeting.Longitude;
            meeting.Radius = meetingUpdateDto.Radius.HasValue ? meetingUpdateDto.Radius.Value : meeting.Radius;

            // Обработка загрузки фотографии для встречи
            if (meetingUpdateDto.Photo != null && meetingUpdateDto.Photo.Length > 0)
            {
                try
                {
                    var fileName = $"{id}_{Guid.NewGuid()}{Path.GetExtension(meetingUpdateDto.Photo.FileName)}";
                    var meetingFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "BeerParty.Data", "Uploads", "MeetingPictures");
                    var filePath = Path.Combine(meetingFolder, fileName);

                    if (!Directory.Exists(meetingFolder))
                    {
                        Directory.CreateDirectory(meetingFolder);
                    }

                    if (!string.IsNullOrEmpty(meeting.PhotoUrl))
                    {
                        var oldPhotoPath = Path.Combine(meetingFolder, Path.GetFileName(meeting.PhotoUrl));
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await meetingUpdateDto.Photo.CopyToAsync(stream);
                    }

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
        [HttpPost("like/{feedbackId}")]
        public async Task<IActionResult> ToggleLike(long feedbackId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var like = await _context.Likes
                .SingleOrDefaultAsync(l => l.UserId == userId && l.MeetingReviewId == feedbackId);

            if (like != null)
            {
                // Удаляем лайк
                _context.Likes.Remove(like);
            }
            else
            {
                // Добавляем лайк
                var newLike = new Like { UserId = userId.Value, MeetingReviewId = feedbackId };
                await _context.Likes.AddAsync(newLike);
            }

            await _context.SaveChangesAsync();
            return Ok("Изменения сохранены");
        }
        [HttpPost("like/meeting/{meetingId}")]
        public async Task<IActionResult> ToggleMeetingLike(long meetingId)
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

        [HttpGet("recommended")]
        public async Task<IActionResult> GetRecommendedMeetings(int page = 0, int pageSize = 10)
        {
            var meetings = await GetRecommendedMeetingsAsync(pageSize);
            return Ok(meetings);
        }

        // Приватный метод для получения рекомендованных встреч
        private async Task<List<Meeting>> GetRecommendedMeetingsAsync(int count)
        {
            var meetings = await _context.Meetings
                .Select(m => new
                {
                    Meeting = m,
                    LikesCount = m.Likes.Count(),
                    ReviewsCount = m.Reviews.Count(),
                    AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0 // Средний рейтинг
                })
                .OrderByDescending(m => m.LikesCount + m.ReviewsCount + (m.AverageRating * 10)) // Сортировка по количеству лайков, отзывов и среднему рейтингу
                .Take(count)
                .ToListAsync();

            return meetings.Select(m => m.Meeting).ToList();
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
      

        // GET: api/MeetingReview/{id}
     

        // POST: api/MeetingReview
        [HttpPost("CreateMeetingReview")]
        public async Task<ActionResult<MeetingReview>> CreateMeetingReview([FromQuery] AddReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = new MeetingReview
            {
                MeetingId = dto.MeetingId,
                UserId = long.Parse(userId), 
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.MeetingReviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeetingReview), new { id = review.Id }, review);
        }

        [HttpGet("MeetingReview/{id}")]
        public async Task<ActionResult<MeetingReview>> GetMeetingReview(long id)
        {
            var review = await _context.MeetingReviews
                .Include(mr => mr.Meeting)
                .Include(mr => mr.User)
                .FirstOrDefaultAsync(mr => mr.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }
        [HttpGet("AllMeetingReview")]
        public async Task<ActionResult<IEnumerable<MeetingReview>>> GetMeetingReviews()
        {
            var reviews = await _context.MeetingReviews
                .Include(mr => mr.Meeting)
                .Include(mr => mr.User)
                .ToListAsync();

            return Ok(reviews);
        }

        // PUT: api/MeetingReview/{id}
        [HttpPut("UpdateMeetingReview")]
        public async Task<ActionResult<MeetingReview>> UpdateMeetingReview([FromQuery] UpdateReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Находим отзыв по ID
            var review = await _context.MeetingReviews
                .SingleOrDefaultAsync(r => r.Id == dto.reviewId && r.UserId == long.Parse(userId));

            if (review == null)
            {
                return NotFound("Отзыв не найден или у вас нет прав для его изменения.");
            }

            // Обновляем только те поля, которые были переданы
            if (dto.Rating.HasValue)
            {
                review.Rating = dto.Rating.Value;
            }

            if (!string.IsNullOrEmpty(dto.Comment))
            {
                review.Comment = dto.Comment;
            }

            // Сохраняем изменения
            await _context.SaveChangesAsync();

            return Ok(review);
        }



        // DELETE: api/MeetingReview/{id}
        [HttpDelete("MeetingReview{id}")]
        public async Task<IActionResult> DeleteMeetingReview(long id)
        {
            var review = await _context.MeetingReviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.MeetingReviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeetingReviewExists(long id)
        {
            return _context.MeetingReviews.Any(e => e.Id == id);
        }

    }
}
