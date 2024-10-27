using BeerParty.BL.Dto;
using BeerParty.Data;
using BeerParty.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Фильтрация по имени пользователя
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            users = users.Where(u => u.Name!.Contains(request.UserName));
        }

        // Фильтрация по имени профиля
        if (!string.IsNullOrWhiteSpace(request.ProfileName))
        {
            users = users.Where(u => u.Profile!.FirstName!.Contains(request.ProfileName));
        }

        // Фильтрация по возрасту
        if (request.MinAge.HasValue)
        {
            users = users.Where(u => u.Profile!.Age >= request.MinAge.Value);
        }

        if (request.MaxAge.HasValue)
        {
            users = users.Where(u => u.Profile!.Age <= request.MaxAge.Value);
        }

        // Фильтрация по полу
        if (request.Gender.HasValue)
        {
            users = users.Where(u => u.Profile!.Gender == request.Gender.Value);
        }

        // Фильтрация по интересам
        if (request.Interests != null && request.Interests.Count > 0)
        {
            users = users.Where(u => u.Profile!.Interests!.Any(i => request.Interests.Contains(i.Id)));
        }

        // Фильтрация по росту
        if (request.MinHeight.HasValue)
        {
            users = users.Where(u => u.Profile!.Height >= request.MinHeight.Value);
        }

        if (request.MaxHeight.HasValue)
        {
            users = users.Where(u => u.Profile!.Height <= request.MaxHeight.Value);
        }

        // Фильтрация по цели поиска
        if (request.Preference.HasValue)
        {
            users = users.Where(u => u.Profile != null && u.Profile.LookingFor == request.Preference.Value);
        }

        var filteredUsers = await users.ToListAsync();
        return Ok(filteredUsers);
    }
}
