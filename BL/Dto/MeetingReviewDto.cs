using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record MeetingReviewDto(long MeetingId, int Rating, string Comment);
}
