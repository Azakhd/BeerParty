using System;

namespace BeerParty.Data.Entities
{

    public class Interest
    {
        public int InterestId { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
    }

}