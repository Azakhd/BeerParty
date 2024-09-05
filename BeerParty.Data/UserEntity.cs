using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public Profile Profile { get; set; }

    }
}

