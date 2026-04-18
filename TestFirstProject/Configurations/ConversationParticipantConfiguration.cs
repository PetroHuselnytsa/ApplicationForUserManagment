using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
    {
        public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
        {
            builder.ToTable("conversation_participants");

            // Composite primary key
            builder.HasKey(cp => new { cp.ConversationId, cp.UserId });

            builder.Property(cp => cp.ConversationId)
                   .HasColumnName("conversation_id");

            builder.Property(cp => cp.UserId)
                   .HasColumnName("user_id");

            builder.Property(cp => cp.JoinedAt)
                   .HasColumnName("joined_at")
                   .IsRequired();

            // Relationships
            builder.HasOne(cp => cp.Conversation)
                   .WithMany(c => c.Participants)
                   .HasForeignKey(cp => cp.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cp => cp.User)
                   .WithMany(u => u.ConversationParticipants)
                   .HasForeignKey(cp => cp.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index for fast user conversation lookups
            builder.HasIndex(cp => cp.UserId)
                   .HasDatabaseName("IX_conversation_participants_user_id");

            // Index for fast conversation participant lookups
            builder.HasIndex(cp => cp.ConversationId)
                   .HasDatabaseName("IX_conversation_participants_conversation_id");
        }
    }
}
