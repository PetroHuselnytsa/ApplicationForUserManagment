using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core configuration for the User entity.
    /// Maps to the "auth_users" table with appropriate indexes and constraints.
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

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired();

            builder.Property(u => u.EmailVerified)
                   .HasColumnName("email_verified")
                   .HasDefaultValue(false);

            builder.Property(u => u.EmailVerificationToken)
                   .HasColumnName("email_verification_token")
                   .HasMaxLength(256);

            builder.Property(u => u.EmailVerificationTokenExpiresAt)
                   .HasColumnName("email_verification_token_expires_at");

            builder.Property(u => u.FailedLoginAttempts)
                   .HasColumnName("failed_login_attempts")
                   .HasDefaultValue(0);

            builder.Property(u => u.LockoutEnd)
                   .HasColumnName("lockout_end");

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("NOW()");

            // Unique index on email for fast lookups and uniqueness enforcement
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("ix_auth_users_email");
        }
    }
}
