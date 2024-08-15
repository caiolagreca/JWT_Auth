using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace JwtAuth
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string email { get; set; }
        public string password { get; set; }

    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(
                new User { Id = 1, email = "caio1@gmail.com", password = "1234" },
                new User { Id = 2, email = "caio2@gmail.com", password = "4321" }
                );
        }

    }
}