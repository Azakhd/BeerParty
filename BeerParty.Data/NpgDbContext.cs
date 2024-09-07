using Microsoft.EntityFrameworkCore;
using BeerParty.Data.Entities;


namespace BeerParty.Data;

	public class NpgDbContext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Profile> Profiles { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<Interest> interests { get; set; }
		public DbSet<Meet> Meets { get; set; }
	
}
