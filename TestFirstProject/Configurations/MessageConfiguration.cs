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
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(m => m.ConversationId)
                   .HasColumnName("conversation_id")
                   .IsRequired();

            builder.Property(m => m.SenderId)
                   .HasColumnName("sender_id")
                   .IsRequired();

            builder.Property(m => m.Content)
                   .HasColumnName("content")
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(m => m.IsRead)
                   .HasColumnName("is_read")
                   .HasDefaultValue(false);

            builder.Property(m => m.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(m => m.ReadAt)
                   .HasColumnName("read_at");

            builder.Property(m => m.IsDeletedBySender)
                   .HasColumnName("is_deleted_by_sender")
                   .HasDefaultValue(false);

            builder.Property(m => m.IsDeletedByRecipient)
                   .HasColumnName("is_deleted_by_recipient")
                   .HasDefaultValue(false);

            // Relationships
            builder.HasOne(m => m.Conversation)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                   .WithMany(u => u.SentMessages)
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict); // Don't cascade-delete messages when user is deleted

            // Index for conversation message lookups (cursor-based pagination)
            builder.HasIndex(m => new { m.ConversationId, m.CreatedAt })
                   .HasDatabaseName("IX_messages_conversation_id_created_at");

            // Index for unread message counts by sender
            builder.HasIndex(m => new { m.ConversationId, m.SenderId, m.IsRead })
                   .HasDatabaseName("IX_messages_conversation_sender_is_read");

            // Index for sender lookups
            builder.HasIndex(m => m.SenderId)
                   .HasDatabaseName("IX_messages_sender_id");
        }
    }
}
