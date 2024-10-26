using BeerParty.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace BeerParty.Data.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string? Name { get; set; }

        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        [Required]
        public DateTime CreatedAt { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public virtual Profile Profile { get; set; }
        public virtual ICollection<MeetingReview> MeetingReviews { get; set; }

        public ICollection<Like> Likes { get; set; }

    }

   


}

