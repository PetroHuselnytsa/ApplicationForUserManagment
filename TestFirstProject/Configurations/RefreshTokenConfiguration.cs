using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("auth_refresh_tokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(rt => rt.Token)
                   .HasColumnName("token")
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(rt => rt.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                   .HasColumnName("is_revoked")
                   .HasDefaultValue(false);

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnName("revoked_at");

            // Unique index on token for fast lookups
            builder.HasIndex(rt => rt.Token)
                   .IsUnique()
                   .HasDatabaseName("IX_auth_refresh_tokens_token");

            // Index on user_id for finding tokens by user
            builder.HasIndex(rt => rt.UserId)
                   .HasDatabaseName("IX_auth_refresh_tokens_user_id");

            // Relationship to User
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
