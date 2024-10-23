using BeerParty.BL.Dto;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BeerParty.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : BaseController
    {
        private readonly ApplicationContext _context;

        public ProfileController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Получение ID пользователя из токена
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Поиск профиля пользователя
            var profile = await _context.Profiles
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.FirstName,
                    p.LastName,
                    p.Bio,
                    p.Location,
                    p.DateOfBirth,
                    p.Gender,
                    PhotoUrl = p.PhotoUrl ?? "default_photo_url" // Устанавливаем значение по умолчанию
                })
                .SingleOrDefaultAsync();

            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            return Ok(profile);
        }
        [HttpPut("update-profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateDto model)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            // Обновление полей профиля
            profile.FirstName = model.FirstName ?? profile.FirstName;
            profile.LastName = model.LastName ?? profile.LastName;
            profile.Bio = model.Bio ?? profile.Bio;
            profile.Location = model.Location ?? profile.Location;
            profile.DateOfBirth = model.DateOfBirth != default(DateTime) ? model.DateOfBirth : profile.DateOfBirth;

            if (model.Gender.HasValue)
            {
                profile.Gender = model.Gender.Value; // Присвоение значения только если оно есть
            }

            // Обработка загрузки фото
            if (model.Photo != null && model.Photo.Length > 0)
            {
                try
                {
                    // Получение инициалов пользователя
                    var firstNameInitial = profile.FirstName.Substring(0, 1).ToUpper();
                    var lastNameInitial = profile.LastName.Substring(0, 1).ToUpper();

                    // Создание уникального имени файла
                    var fileName = $"{firstNameInitial}{lastNameInitial}_{userId}{Path.GetExtension(model.Photo.FileName)}";

                    // Путь к папке Uploads в проекте BeerParty.Data
                    var userFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "BeerParty.Data", "Uploads", "ProfilePictures", userId.ToString());
                    var filePath = Path.Combine(userFolder, fileName);

                    // Создать папку, если она не существует
                    if (!Directory.Exists(userFolder))
                    {
                        Directory.CreateDirectory(userFolder);
                    }

                    // Удаление старого фото, если оно существует
                    if (!string.IsNullOrEmpty(profile.PhotoUrl))
                    {
                        var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "BeerParty.Data", "Uploads", "ProfilePictures", userId.ToString(), Path.GetFileName(profile.PhotoUrl));
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    // Сохранение нового файла
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(stream);
                    }

                    // URL для доступа к загруженному файлу
                    profile.PhotoUrl = $"{Request.Scheme}://{Request.Host}/uploads/ProfilePictures/{userId}/{fileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Ошибка при загрузке фото: {ex.Message}");
                }
            }

            // Обновление интересов
            profile.Interests.Clear();
            if (model.InterestIds != null && model.InterestIds.Any())
            {
                foreach (var interestId in model.InterestIds)
                {
                    var interest = await _context.Interests.FindAsync(interestId);
                    if (interest != null)
                    {
                        profile.Interests.Add(interest);
                    }
                }
            }

            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
            return Ok("Профиль обновлён");
        }
    }
}
