using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core configuration for the User entity.
    /// Maps to the "auth_users" table with indexes for performance.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("auth_users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .IsRequired()
                   .HasMaxLength(256);

            // Unique index on email for fast lookups and uniqueness enforcement
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_auth_users_email");

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(u => u.EmailConfirmed)
                   .HasColumnName("email_confirmed")
                   .HasDefaultValue(false);

            builder.Property(u => u.EmailVerificationToken)
                   .HasColumnName("email_verification_token")
                   .HasMaxLength(512);

            builder.Property(u => u.EmailVerificationTokenExpiry)
                   .HasColumnName("email_verification_token_expiry");

            builder.Property(u => u.LockoutEnd)
                   .HasColumnName("lockout_end");

            builder.Property(u => u.AccessFailedCount)
                   .HasColumnName("access_failed_count")
                   .HasDefaultValue(0);

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("NOW()");
        }
    }
}
