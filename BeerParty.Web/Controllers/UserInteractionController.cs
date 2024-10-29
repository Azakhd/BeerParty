using BeerParty.BL.Dto; // Ваши DTO
using BeerParty.Data.Entities; // Ваши сущности
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeerParty.Data.Enums;

namespace BeerParty.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInteractionController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public UserInteractionController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("like")]
        public async Task<IActionResult> Like([FromBody] InteractionDto dto)
        {
            // Проверка существования взаимодействия
            var existingInteraction = await _context.ProfileInteractions
                .FirstOrDefaultAsync(i => i.FromProfileId == dto.FromProfileId && i.ToProfileId == dto.ToProfileId);

            if (existingInteraction != null)
            {
                return BadRequest("Взаимодействие уже существует.");
            }

            var interaction = new ProfileInteraction
            {
                FromProfileId = dto.FromProfileId,
                ToProfileId = dto.ToProfileId,
                Type = InteractionType.Like, // Или другой тип, в зависимости от вашего enum
                Timestamp = DateTime.UtcNow
            };

            _context.ProfileInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("dislike")]
        public async Task<IActionResult> Dislike([FromBody] InteractionDto dto)
        {
            // Проверка существования взаимодействия
            var existingInteraction = await _context.ProfileInteractions
                .FirstOrDefaultAsync(i => i.FromProfileId == dto.FromProfileId && i.ToProfileId == dto.ToProfileId);

            if (existingInteraction != null)
            {
                return BadRequest("Взаимодействие уже существует.");
            }

            var interaction = new ProfileInteraction
            {
                FromProfileId = dto.FromProfileId,
                ToProfileId = dto.ToProfileId,
                Type = InteractionType.Dislike, // Или другой тип, в зависимости от вашего enum
                Timestamp = DateTime.UtcNow
            };

            _context.ProfileInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveInteraction([FromBody] InteractionDto dto)
        {
            var interaction = await _context.ProfileInteractions
                .FirstOrDefaultAsync(i => i.FromProfileId == dto.FromProfileId && i.ToProfileId == dto.ToProfileId);

            if (interaction == null)
            {
                return NotFound("Взаимодействие не найдено.");
            }

            _context.ProfileInteractions.Remove(interaction);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetInteractions(int profileId)
        {
            var interactions = await _context.ProfileInteractions
                .Where(i => i.FromProfileId == profileId || i.ToProfileId == profileId)
                .ToListAsync();

            return Ok(interactions);
        }
    }
}
