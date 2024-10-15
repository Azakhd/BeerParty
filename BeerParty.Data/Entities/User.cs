

using BeerParty.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.ComponentModel.DataAnnotations;

namespace BeerParty.Data.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string ?Name { get; set; }

        [EmailAddress]
        [Required]
        public string ?Email { get; set; }
       
        [Required]
        public string ?PasswordHash { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public Profile? Profile { get; set; }
       

       
    }


}

