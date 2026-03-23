using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
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

            builder.Property(rt => rt.Token)
                   .HasColumnName("token")
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasIndex(rt => rt.Token)
                   .IsUnique();

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("NOW()");

            builder.Property(rt => rt.IsRevoked)
                   .HasColumnName("is_revoked")
                   .HasDefaultValue(false);

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnName("revoked_at");
        }
    }
}
