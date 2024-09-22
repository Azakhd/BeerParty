using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class Friend:BaseEntity
    {
        public long UserId { get; set; } // Идентификатор пользователя, который добавил друга
        public long FriendId { get; set; } // Идентификатор друга
        public DateTime FriendshipDate { get; set; }
        public User ?User { get; set; } // Пользователь, который добавил друга
        public User ?FriendUser { get; set; } // Друг, который был добавлен
    }
}
