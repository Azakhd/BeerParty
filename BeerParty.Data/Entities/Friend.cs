using BeerParty.Data.Enums;

namespace BeerParty.Data.Entities
{
    public class Friend:BaseEntity
    {
        public long UserId { get; set; } // Пользователь, который добавил друга
        public User ?User { get; set; } // Связь с пользователем
        public long FriendId { get; set; } // Друг
        public User ?FriendUser { get; set; } // Связь с другом
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    }
}
