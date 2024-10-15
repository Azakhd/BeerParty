using BeerParty.BL.Dto;
using BeerParty.Data;
using BeerParty.Data.Entities;
using BeerParty.Data.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeerParty.Web.Controllers
{
    public class AccountController:BaseController
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            // Хеширование пароля
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var profile = new Profile
            {
                UserId = user.Id,
                FirstName = model.Name,
                // другие поля по умолчанию
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Name == model.Name);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            // Генерируем JWT токен
            var token = GenerateJwtToken(user);

            // Возвращаем токен клиенту
            return Ok(new { token });
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        // Используем ClaimTypes.Name для имени пользователя
        new Claim(ClaimTypes.Name, user.Name!), // Имя пользователя
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID пользователя
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, string.Join(", ", user.Roles)) // Роли пользователя
    };

            // Отладочная информация
            Console.WriteLine($"Generated claims: {string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}"))}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        [Authorize(Roles = "Admin")] // Убедитесь, что только админы могут вызывать этот метод
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Получение идентификатора текущего пользователя
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Проверяем, если идентификатор не найден
            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return BadRequest("Invalid user ID.");
            }

            // Получение пользователей, исключая текущего по идентификатору
            var users = await _context.Users
                .Where(u => u.Id.ToString() != currentUserIdString) // Исключаем текущего пользователя по ID
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = string.Join(", ", u.Roles) // Преобразование списка ролей в строку
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-role")]
        public async Task<IActionResult> UpdateUserRole(long userId, Role newRole)
        {
            // Проверка, что текущий пользователь является администратором
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return Unauthorized("Текущий пользователь не авторизован.");
            }

            // Поиск пользователя, роль которого нужно изменить
            var user = await _context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            // Изменение роли пользователя
            if (!user.Roles.Contains(newRole))
            {
                user.Roles.Clear(); // Очистим предыдущие роли (если у вас только одна роль)
                user.Roles.Add(newRole); // Добавим новую роль
            }

            // Сохранение изменений
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Роль пользователя {user.Name} изменена на {newRole}." });
        }


        [Authorize]
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

    }
}
