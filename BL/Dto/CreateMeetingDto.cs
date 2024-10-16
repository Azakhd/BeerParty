﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record CreateMeetingDto(DateTime MeetingTime, string Location,
        string Description, List<int> ParticipantIds, string Title);
    
}