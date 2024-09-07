using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BeerParty.Data.Entities;


namespace BeerParty.Data.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public UserConfigurations() { }

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);

         
             
                

        }
    }
}
