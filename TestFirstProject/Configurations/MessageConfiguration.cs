using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(m => m.ConversationId)
                   .HasColumnName("conversation_id");

            builder.Property(m => m.SenderId)
                   .HasColumnName("sender_id");

            builder.Property(m => m.Content)
                   .HasColumnName("content")
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(m => m.IsRead)
                   .HasColumnName("is_read")
                   .HasDefaultValue(false);

            builder.Property(m => m.IsDeletedBySender)
                   .HasColumnName("is_deleted_by_sender")
                   .HasDefaultValue(false);

            builder.Property(m => m.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(m => m.ReadAt)
                   .HasColumnName("read_at");

            // Relationships
            builder.HasOne(m => m.Conversation)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                   .WithMany(u => u.SentMessages)
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common queries
            builder.HasIndex(m => m.ConversationId);
            builder.HasIndex(m => m.SenderId);
            builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });
            builder.HasIndex(m => new { m.ConversationId, m.IsRead });
        }
    }
}
