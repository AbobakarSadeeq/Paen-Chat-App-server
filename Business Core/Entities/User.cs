﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string? FullName { get; set; }
       // public string? Email { get; set; }
       // public string VerificationPassword { get; set; }
        public string ContactNumber { get; set; }
        public string? AboutStatus { get; set; }
       // public bool EmailVerification { get; set; } // used for when user logging then email is verfiy and logged in then true and when logout then become false and it will be true when user is logged in 
       // public bool isActive { get; set; }

        public string? UserGroupPrivateConnectionIds { get; set; }
        /// <summary>
        /// one user have only one profile photo
        /// </summary>
        /// 
        public string? ProfilePhotoUrl { get; set; }
      //  public string? PublicId { get; set; }

        public DateTime? LastOnline { get; set; }
        public DateTime? CreatedAt { get; set; }

       // public virtual ICollection<Contact> Contacts { get; set; }
     //   public virtual ICollection<Message> SenderMessages { get; set; }
     //   public virtual ICollection<Message> ReciverMessage { get; set; }

    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.UserID);
            builder.HasIndex(u => u.ContactNumber)
                 .IsUnique();


        }
    }
}
