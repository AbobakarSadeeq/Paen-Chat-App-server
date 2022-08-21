using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Contact
    {
        public int ContactID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LastMessage { get; set; }
        public bool Verified_Contact { get; set; }
        public bool Block_Contact { get; set; }
        public DateTime? Created_At { get; set; }
        public int UserId { get; set; }
        public virtual User? User { get; set; }

    }

    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.HasKey(a => a.ContactID);

            // Relationship
            builder.HasOne(a => a.User)
                .WithMany(a => a.Contacts)
                .HasForeignKey(a=>a.UserId)
                .IsRequired(true);
        }
    }
}
