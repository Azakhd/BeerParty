using BeerParty.Data.Enums;
using System;

namespace BeerParty.Data.Entities
{
    public class Meet : BaseEntity
    {
        public class MeetEntity : BaseEntity
        {
            public string? Title { get; set; } // Nullable string
            public string? Description { get; set; }
            public string? Location { get; set; }
            public MeetCategory Category { get; set; } = MeetCategory.Default; // Знач
            public List<User>? InvitedUsers { get; set; } = new List<User>(); // Инициализация пустым списком

            // Связь с организатором
            public long OrganizerId { get; set; }
            public User ?Organizer { get; set; }
        }
}

} 