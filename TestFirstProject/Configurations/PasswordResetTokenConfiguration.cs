using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core fluent API configuration for the PasswordResetToken entity.
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("password_reset_tokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(t => t.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(t => t.Token)
                   .HasColumnName("token")
                   .IsRequired();

            builder.Property(t => t.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(t => t.IsUsed)
                   .HasColumnName("is_used")
                   .HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            builder.HasIndex(t => t.Token)
                   .IsUnique()
                   .HasDatabaseName("ix_password_reset_tokens_token");

            builder.HasOne(t => t.User)
                   .WithMany(u => u.PasswordResetTokens)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
