using System;

namespace BeerParty.Data
{

    public class Interest
    {
        public int InterestId { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
    }

}