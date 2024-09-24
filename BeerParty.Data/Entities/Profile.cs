namespace BeerParty.Data.Entities
{
    public class Profile:BaseEntity
    {

        public long UserId { get; set; }
        public User ?User { get; set; }
        public string? PhotoUrl { get; set; }
        public ICollection<Friend> ?Friends { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public ICollection<Interest> ?Interests { get; set; }
      

    }
}

