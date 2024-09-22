using BeerParty.Data.Enums;
using System;

namespace BeerParty.Data.Entities
{

    public class Interest : BaseEntity
    {
        public string? Name { get; set; }
        public InterestCategory Category { get; set; } // Использование enum для категории интереса
        public ICollection<UserInterest> ?UserInterests { get; set; }
    }

}