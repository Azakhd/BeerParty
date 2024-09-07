namespace BeerParty.Data.Entities
{
    public class Profile
    {

        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public ICollection<Interest> Interests { get; set; }
        public string ProfilePictureUrl { get; set; }

    }
}

