using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class MeetingReview : BaseEntity
    {
        public long MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public int Rating { get; set; } // Оценка от 1 до 5
        public string? Comment { get; set; } // Комментарий к встрече
    }
}
