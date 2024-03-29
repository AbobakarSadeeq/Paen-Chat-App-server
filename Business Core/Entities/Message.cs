﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Entities
{
    public class Message
    {
        public int MessageID { get; set; }
        public int? SenderId { get; set; }
        public virtual User? SenderUser { get; set; }
        public int? ReceiverId { get; set; }
        public virtual User? ReciverUser { get; set; }

        public string? UserMessage { get; set; } // by default null
        public DateTime? Created_At { get; set; }
        public int MessageSeen { get; set; } // 0 => offline, 1 => online, 3 => specific connected with page

        public ICollection<MessageAttachment> MessageAttachments { get; set; }
    }

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(a => a.MessageID);

            // Relationship sender
            //builder.HasOne(a => a.SenderUser)
            //    .WithMany(a => a.SenderMessages)
            //    .HasForeignKey(a => a.SenderId);



            //// Relationship reciver
            //builder.HasOne(a => a.ReciverUser)
            //    .WithMany(a => a.ReciverMessage)
            //    .HasForeignKey(a => a.ReceiverId);





        }
    }
}
