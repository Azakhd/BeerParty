using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class UserInterest
    {
        public long UserId { get; set; }
        public User ?User { get; set; } // Пользователь, который имеет интерес

        public long InterestId { get; set; }
        public Interest ?Interest { get; set; } // Интерес пользователя
    }
}
