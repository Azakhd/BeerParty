using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public class InteractionDto
    {
        public int FromProfileId { get; set; }
        public int ToProfileId { get; set; }
    }
}
