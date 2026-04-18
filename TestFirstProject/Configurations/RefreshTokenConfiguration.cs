using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core configuration for the RefreshToken entity.
    /// Maps to the "auth_refresh_tokens" table with indexes for fast token lookups.
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("auth_refresh_tokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(rt => rt.Token)
                   .HasColumnName("token")
                   .IsRequired()
                   .HasMaxLength(512);

            // Index on token for fast lookups during refresh flow
            builder.HasIndex(rt => rt.Token)
                   .IsUnique()
                   .HasDatabaseName("IX_auth_refresh_tokens_token");

            builder.Property(rt => rt.UserId)
                   .HasColumnName("user_id");

            // Index on user_id for finding all tokens belonging to a user
            builder.HasIndex(rt => rt.UserId)
                   .HasDatabaseName("IX_auth_refresh_tokens_user_id");

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnName("revoked_at");

            // Ignore computed properties — they are not mapped to DB columns
            builder.Ignore(rt => rt.IsRevoked);
            builder.Ignore(rt => rt.IsExpired);
            builder.Ignore(rt => rt.IsActive);

            // Relationship: RefreshToken -> User
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
