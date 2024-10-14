using BeerParty.BL.Dto;
using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace BeerParty.Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ApplicationContext _context;

        public ProfileController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // Получите текущий профиль пользователя и передайте его в представление
            var userId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserId == userId);
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

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

            // Обновите интересы, если необходимо
            if (model.InterestIds != null)
            {
                profile.Interests = await _context.Interests
                    .Where(i => model.InterestIds.Contains(i.Id)).ToListAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ProfileUpdated");
        }

        public IActionResult ProfileUpdated()
        {
            return View();
        }
    }
}
