using BeerParty.BL.Dto;
using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BeerParty.Web.Controllers
{
    public class UserSearchController : BaseController
    {
        private readonly ApplicationContext _context;

        public UserSearchController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromForm] UserSearchDto request)
        {
            var users = _context.Users.Include(u => u.Profile).AsQueryable();

            // Фильтрация по имени
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                users = users.Where(u => u.Name!.Contains(request.Name));
            }

            // Фильтрация по возрасту
            if (request.MinAge.HasValue)
            {
                users = users.Where(u => u.Profile!.Age >= request.MinAge.Value);
            }

            if (request.MaxAge.HasValue)
            {
                users = users.Where(u => u.Profile!.Age! <= request.MaxAge.Value);
            }

            // Фильтрация по полу
            if (request.Gender.HasValue)
            {
                users = users.Where(u => u.Profile!.Gender == request.Gender.Value);
            }

            // Учет интересов из профиля
            if (request.Interests != null && request.Interests.Count > 0)
            {
                users = users.Where(u => u.Profile!.Interests!.Any(i => request.Interests.Contains(i.Id)));
            }

            // Учет роста
            if (request.MinHeight.HasValue)
            {
                users = users.Where(u => u.Profile!.Height >= request.MinHeight.Value);
            }

            if (request.MaxHeight.HasValue)
            {
                users = users.Where(u => u.Profile!.Height <= request.MaxHeight.Value);
            }

            // Получаем список отфильтрованных пользователей
            var filteredUsers = await users.ToListAsync();

            // Возвращаем результаты поиска как JSON
            return Ok(filteredUsers);
        }
    }
}
