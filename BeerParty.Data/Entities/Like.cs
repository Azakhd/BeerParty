using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class Like:BaseEntity
    {
        public long MeetingId { get; set; }
        public virtual Meeting Meeting { get; set; }
        public long UserId { get; set; } // Убедитесь, что это свойство сущес
        public User User { get; set; }
    }
}
