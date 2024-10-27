using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class MeetingLike : BaseEntity
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public long MeetingId { get; set; }
        public Meeting Meeting { get; set; }
    }
}
