using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class MeetingReviewLike : BaseEntity
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public long MeetingReviewId { get; set; }
        public MeetingReview MeetingReview { get; set; }
    }
}
