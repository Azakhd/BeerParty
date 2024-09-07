

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace BeerParty.Data.Entities
{
    public class User
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public Profile Profile { get; set; }

    }

   
}

