using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Entities
{
    public class MessageAttachment
    {
        public int MessageAttachmentID { get; set; }
        public string? URL { get; set; }
        public string? PublicId { get; set; }
        public int MessageId { get; set; }
        public virtual Message? Message { get; set; }


    }

    public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
    {
        public void Configure(EntityTypeBuilder<MessageAttachment> builder)
        {
            builder.HasKey(a => a.MessageAttachmentID);

            // Relationship reciver
            builder.HasOne(a => a.Message)
                .WithMany(a => a.MessageAttachments)
                .HasForeignKey(a => a.MessageId)
                .IsRequired(true);
        }
    }
}
