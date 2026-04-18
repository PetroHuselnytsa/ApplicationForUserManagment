using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(n => n.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(n => n.Type)
                   .HasColumnName("type")
                   .IsRequired()
                   .HasConversion<int>(); // Store enum as int

            builder.Property(n => n.Title)
                   .HasColumnName("title")
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(n => n.Body)
                   .HasColumnName("body")
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(n => n.IsRead)
                   .HasColumnName("is_read")
                   .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(n => n.ReadAt)
                   .HasColumnName("read_at");

            builder.Property(n => n.ReferenceId)
                   .HasColumnName("reference_id")
                   .HasMaxLength(256);

            // Relationship
            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index for user notification lookups (paginated list)
            builder.HasIndex(n => new { n.UserId, n.CreatedAt })
                   .HasDatabaseName("IX_notifications_user_id_created_at");

            // Index for unread notification counts
            builder.HasIndex(n => new { n.UserId, n.IsRead })
                   .HasDatabaseName("IX_notifications_user_id_is_read");
        }
    }
}
