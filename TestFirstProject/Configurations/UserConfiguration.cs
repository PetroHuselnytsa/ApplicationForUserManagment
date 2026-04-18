using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core fluent API configuration for the User entity.
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
                   .HasMaxLength(50);

            builder.Property(u => u.Role)
                   .HasColumnName("role")
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(u => u.IsPremium)
                   .HasColumnName("is_premium")
                   .HasDefaultValue(false);

            builder.Property(u => u.IsEmailVerified)
                   .HasColumnName("is_email_verified")
                   .HasDefaultValue(false);

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
