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
                   .ValueGeneratedOnAdd();

            builder.Property(np => np.UserId)
                   .HasColumnName("user_id");

            builder.Property(np => np.NewMessageEnabled)
                   .HasColumnName("new_message_enabled")
                   .HasDefaultValue(true);

            builder.Property(np => np.RoleChangedEnabled)
                   .HasColumnName("role_changed_enabled")
                   .HasDefaultValue(true);

            builder.Property(np => np.SystemAlertEnabled)
                   .HasColumnName("system_alert_enabled")
                   .HasDefaultValue(true);

            builder.Property(np => np.UserMentionedEnabled)
                   .HasColumnName("user_mentioned_enabled")
                   .HasDefaultValue(true);

            // One-to-one relationship
            builder.HasOne(np => np.User)
                   .WithOne(u => u.NotificationPreference)
                   .HasForeignKey<NotificationPreference>(np => np.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(np => np.UserId).IsUnique();
        }
    }
}
