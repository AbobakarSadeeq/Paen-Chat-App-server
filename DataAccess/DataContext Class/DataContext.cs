using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DataContext_Class
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Message> Messages{ get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<User> Users{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Message>(new MessageConfiguration());
            modelBuilder.ApplyConfiguration<MessageAttachment>(new MessageAttachmentConfiguration());
            modelBuilder.ApplyConfiguration<Contact>(new ContactConfiguration());
            modelBuilder.ApplyConfiguration<User>(new UserConfiguration());

        }


    }
}
