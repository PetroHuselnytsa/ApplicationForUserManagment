using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("notification_preferences");

            builder.HasKey(np => np.Id);

            builder.Property(np => np.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(np => np.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(np => np.NotificationType)
                   .HasColumnName("notification_type")
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(np => np.IsEnabled)
                   .HasColumnName("is_enabled")
                   .HasDefaultValue(true);

            // Relationship
            builder.HasOne(np => np.User)
                   .WithMany(u => u.NotificationPreferences)
                   .HasForeignKey(np => np.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique index: one preference per notification type per user
            builder.HasIndex(np => new { np.UserId, np.NotificationType })
                   .IsUnique()
                   .HasDatabaseName("IX_notification_preferences_user_type");
        }
    }
}
