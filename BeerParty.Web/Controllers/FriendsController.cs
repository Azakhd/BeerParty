using BeerParty.Data.Entities;
using BeerParty.Data;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BeerParty.Data.Enums;

namespace BeerParty.Web.Controllers
{

    public class FriendsController : BaseController
    {
        private readonly ApplicationContext _context;

        public FriendsController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("add-friend-request/{friendId}")]
        public async Task<IActionResult> AddFriendRequest(long friendId)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdString) || !long.TryParse(currentUserIdString, out long currentUserId))
            {
                return BadRequest("Invalid user ID.");
            }

            var friend = await _context.Users.FindAsync(friendId);
            if (friend == null)
            {
                return NotFound("Friend not found.");
            }

            var existingFriendRequest = await _context.Friends
                .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == friendId) ||
                               (f.UserId == friendId && f.FriendId == currentUserId));

            if (existingFriendRequest)
            {
                return BadRequest("Friend request already exists.");
            }

            var friendRequest = new Friend
            {
                UserId = currentUserId,
                FriendId = friendId,
                Status = FriendshipStatus.Pending
            };

            _context.Friends.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Ok("Friend request sent successfully.");
        }

        [HttpPost("respond-to-friend-request/{friendId}")]
        public async Task<IActionResult> RespondToFriendRequest(long friendId, [FromQuery] bool accept)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdString) || !long.TryParse(currentUserIdString, out long currentUserId))
            {
                return BadRequest("Invalid user ID.");
            }

            var friendRequest = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == currentUserId && f.Status == FriendshipStatus.Pending);

            if (friendRequest == null)
            {
                return NotFound("Friend request not found.");
            }

            friendRequest.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok(accept ? "Friend request accepted." : "Friend request rejected.");
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetFriends()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return BadRequest("Invalid user ID.");
            }

            var friends = await _context.Friends
                .Where(f => (f.UserId == currentUserId || f.FriendId == currentUserId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => new
                {
                    FriendId = f.UserId == currentUserId ? f.FriendId : f.UserId,
                    FriendName = f.UserId == currentUserId ? f.FriendUser!.Name : f.User.Name,
                    FriendEmail = f.UserId == currentUserId ? f.FriendUser!.Email : f.User.Email
                })
                .ToListAsync();

            return Ok(friends);
        }

    }


}
