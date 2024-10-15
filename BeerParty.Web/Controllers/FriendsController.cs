using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BeerParty.Web.Controllers
{

    public class FriendsController : BaseController
    {
        private readonly ApplicationContext _context;

        public FriendsController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("add-friend/{friendId}")]
        public async Task<IActionResult> AddFriend(long friendId)
        {
            // Извлечение идентификатора текущего пользователя из клейма
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return BadRequest("User ID not found in claims.");
            }

            // Преобразование идентификатора в long
            if (!long.TryParse(currentUserIdString, out long currentUserId))
            {
                return BadRequest("Invalid user ID.");
            }

            var friend = await _context.Users.FindAsync(friendId);
            if (friend == null)
            {
                return NotFound("Friend not found.");
            }

            var existingFriendship = await _context.Friends
                .AnyAsync(f => f.UserId == currentUserId && f.FriendId == friendId);

            if (existingFriendship)
            {
                return BadRequest("You are already friends.");
            }

            var newFriendship = new Friend
            {
                UserId = currentUserId,
                FriendId = friendId
            };

            _context.Friends.Add(newFriendship);
            await _context.SaveChangesAsync();

            return Ok("Friend added successfully.");
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authorized");
            }

            if (!long.TryParse(userId, out long currentUserId))
            {
                return BadRequest("Invalid user ID.");
            }

            var currentUser = await _context.Users.SingleOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var friends = await _context.Friends
                .Where(f => f.UserId == currentUserId)
                .Include(f => f.FriendUser)
                .Select(f => new
                {
                    f.FriendUser!.Name,
                    f.FriendUser.Email
                })
                .ToListAsync();

            return Ok(friends);
        }
    }


}
