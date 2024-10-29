using BeerParty.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        public Role Role { get; set; } = Role.User;
        [JsonIgnore]
        public virtual Profile Profile { get; set; }
        public virtual ICollection<MeetingReview> MeetingReviews { get; set; }

        public ICollection<MeetingLike> MeetingLikes { get; set; }

        // Связь с лайками для обзоров встреч
        public ICollection<MeetingReviewLike> MeetingReviewLikes { get; set; }

    }


}

