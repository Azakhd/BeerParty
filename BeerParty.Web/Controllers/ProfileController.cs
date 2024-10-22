using BeerParty.BL.Dto;
using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

            // Поиск профиля, связанного с пользователем
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            // Возвращаем профиль как JSON, включая PhotoUrl
            return Ok(profile);
        }


        [HttpPut("update-profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateDto model)
        {
            // Получение ID пользователя из токена
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Поиск профиля, связанного с пользователем
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            // Обновление полей профиля
            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.Bio = model.Bio;
            profile.Location = model.Location;
            profile.DateOfBirth = model.DateOfBirth;
            profile.Gender = model.Gender;

            // Обработка загруженного фото
            if (model.Photo != null && model.Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine("Uploads", "ProfilePictures");
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                profile.PhotoUrl = $"{Request.Scheme}://{Request.Host}/{uploadsFolder}/{fileName}";
            }

            // Обновление интересов
            profile.Interests.Clear(); // Очистить текущие интересы
            foreach (var interestId in model.InterestIds)
            {
                var interest = await _context.Interests.FindAsync(interestId);
                if (interest != null)
                {
                    profile.Interests.Add(interest);
                }
            }

            // Сохранение изменений
            await _context.SaveChangesAsync();
            return Ok("Профиль обновлён");
        }

    }
}
