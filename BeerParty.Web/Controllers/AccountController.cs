using BeerParty.BL.Dto;
using BeerParty.Data;
using BeerParty.Data.Entities;
using Microsoft.AspNetCore.Authentication;
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
            if (await _context.Users.AnyAsync(u => u.Name == model.Name))
                return BadRequest("Username already exists");

            var user = new User
            {
                Name = model.Name,
                Email = model.Email
            };

            // Хеширование пароля
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Name == model.Name);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            // Установите имя пользователя в контексте
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Name!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var claimsIdentity = new ClaimsIdentity(claims, "login");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Name !),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Установите нужное время жизни токена
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            // Получаем имя текущего пользователя из токена
            var currentUserName = User.Identity?.Name;

            if (currentUserName == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Получаем всех пользователей, кроме текущего
            var users = _context.Users
                .Where(u => u.Name != currentUserName) // Фильтруем пользователей, исключая текущего
                .Select(u => u.Name)
                .ToList();

            // Логирование
            Console.WriteLine($"Текущий пользователь: {currentUserName}, Количество пользователей: {users.Count}");

            return Ok(users);
        }

    }
}
