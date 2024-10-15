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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userIdString == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            return Ok(profile); // Возвращаем профиль как JSON
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateDto model)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = long.TryParse(userIdString, out var parsedUserId) ? parsedUserId : (long?)null;

            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound("Профиль не найден");
            }

            // Обновите профиль
            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.Bio = model.Bio;
            profile.Location = model.Location;
            profile.PhotoUrl = model.PhotoUrl;
            profile.Height = model.Height;
            profile.DateOfBirth = model.DateOfBirth; // Установка даты рождения
            profile.Gender = model.Gender; // Установка пола

            // Обновите интересы, если необходимо
            if (model.InterestIds != null)
            {
                profile.Interests = await _context.Interests
                    .Where(i => model.InterestIds.Contains(i.Id)).ToListAsync();
            }

            await _context.SaveChangesAsync();
            return Ok("Профиль обновлён"); // Возвращаем сообщение об успешном обновлении
        }

    }
}
