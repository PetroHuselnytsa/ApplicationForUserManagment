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
                   .ValueGeneratedOnAdd();

            builder.Property(n => n.UserId)
                   .HasColumnName("user_id");

            builder.Property(n => n.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(n => n.Title)
                   .HasColumnName("title")
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(n => n.Body)
                   .HasColumnName("body")
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(n => n.IsRead)
                   .HasColumnName("is_read")
                   .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(n => n.ReadAt)
                   .HasColumnName("read_at");

            // Relationships
            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes for efficient queries
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => new { n.UserId, n.IsRead });
            builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        }
    }
}
