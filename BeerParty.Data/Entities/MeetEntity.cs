using System;
namespace BeerParty.Data.Entities
{
    public class Meet : BaseEntity
    {
        public int HostId { get; set; }
        public string ?Location { get; set; }
        public DateTime DateTime { get; set; }
        public User ?Host { get; set; }
        public ICollection<User> ?Participants { get; set; }
    }
}