using BeerParty.BL.Dto;
using BeerParty.Data;
using BeerParty.Data.Entities;
using BeerParty.Data.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
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
        public async Task<IActionResult> Register(RegisterUserDto model)
        {
            // Проверка на валидность
            if (string.IsNullOrEmpty(model.Name) || model.Name.Length < 2 || model.Name.Length > 50)
            {
                return BadRequest("Имя должно быть от 2 до 50 символов.");
            }

            if (string.IsNullOrEmpty(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
            {
                return BadRequest("Некорректный формат Email.");
            }

            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6 || model.Password.Length > 100)
            {
                return BadRequest("Пароль должен быть от 6 до 100 символов.");
            }

            // Проверка на существование пользователя с таким email
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return Conflict("Пользователь с таким email уже существует.");
            }

            // Создание нового пользователя
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Role.User // Установка роли по умолчанию
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Сохранение пользователя

            // Создание профиля с заполненным только именем
            var profile = new Profile
            {
                UserId = user.Id,
                FirstName = model.Name, // Заполняем имя
                Bio = null, // Устанавливаем Bio как null (или уберите это поле, если оно не нужно)
                PhotoUrl = null,
                LastName = null,
                Location = null,
                Height = null,
                DateOfBirth = default, // Или укажите значение по умолчанию
                Gender = Gender.Other, // Установите значение по умолчанию, если необходимо
                LookingFor = null
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync(); // Сохранение профиля

            return Ok(new { message = "Регистрация успешна", userId = user.Id });
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
        new Claim(ClaimTypes.Role, string.Join(", ", user.Role)) // Роли пользователя
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
                    Role = u.Role.ToString() // Преобразование одиночного значения Role в строку
                })
                .ToListAsync();

            return Ok(users);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("update-role/{userId}")]
        public async Task<IActionResult> UpdateUserRole(long userId, Role newRole)
        {
            // Получение ID текущего пользователя из токена
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return Unauthorized("Текущий пользователь не авторизован.");
            }

            // Поиск пользователя, роль которого нужно изменить
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            // Изменение роли пользователя, если новая роль отличается от текущей
            if (user.Role != newRole)
            {
                user.Role = newRole; // Присваиваем новую роль
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
