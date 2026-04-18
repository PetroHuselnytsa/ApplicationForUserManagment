using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core fluent API configuration for the RefreshToken entity.
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(rt => rt.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(rt => rt.TokenHash)
                   .HasColumnName("token_hash")
                   .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                   .HasColumnName("is_revoked")
                   .HasDefaultValue(false);

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnName("revoked_at");

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            // Index on token hash for fast lookup during refresh
            builder.HasIndex(rt => rt.TokenHash)
                   .IsUnique()
                   .HasDatabaseName("ix_refresh_tokens_token_hash");

            // Index on user_id for finding all tokens belonging to a user
            builder.HasIndex(rt => rt.UserId)
                   .HasDatabaseName("ix_refresh_tokens_user_id");

            // Foreign key to User with cascade delete
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
