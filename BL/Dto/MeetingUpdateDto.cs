using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record MeetingUpdateDto( string? Title, DateTime MeetingTime ,string? Location,
    string? Description ,bool IsPublic, long? CoAuthorId, short? ParticipantLimit);
   
}
