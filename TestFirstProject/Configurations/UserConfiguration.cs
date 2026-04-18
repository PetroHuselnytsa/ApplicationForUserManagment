using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("auth_users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired();

            builder.Property(u => u.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.IsEmailVerified)
                   .HasColumnName("is_email_verified")
                   .HasDefaultValue(false);

            builder.Property(u => u.EmailVerificationToken)
                   .HasColumnName("email_verification_token")
                   .HasMaxLength(256);

            builder.Property(u => u.EmailVerificationTokenExpiresAt)
                   .HasColumnName("email_verification_token_expires_at");

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(u => u.FailedLoginAttempts)
                   .HasColumnName("failed_login_attempts")
                   .HasDefaultValue(0);

            builder.Property(u => u.LockoutEndAt)
                   .HasColumnName("lockout_end_at");

            // Unique index on email for fast lookups and uniqueness enforcement
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_auth_users_email");
        }
    }
}
