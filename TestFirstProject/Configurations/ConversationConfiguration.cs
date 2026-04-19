using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("conversations");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(c => c.LastMessageAt)
                   .HasColumnName("last_message_at");

            // Index on last_message_at for sorting conversations by recent activity
            builder.HasIndex(c => c.LastMessageAt);
        }
    }
}
