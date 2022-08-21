using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class UserProfilePicture
    {
        public int UserProfilePictureID { get; set; }
        public string? URL { get; set; }
        public string? PublicId { get; set; }
        public int UserId { get; set; }
        public virtual User? User { get; set; }



    }

    public class UserProfilePictureConfiguration : IEntityTypeConfiguration<UserProfilePicture>
    {
        public void Configure(EntityTypeBuilder<UserProfilePicture> builder)
        {
            builder.HasKey(a => a.UserProfilePictureID);

            builder.HasOne(a => a.User)
                .WithOne(a => a.UserProfilePicture)
                .HasForeignKey<UserProfilePicture>(a => a.UserId)
                .IsRequired(true);
                

        }
    }
}
