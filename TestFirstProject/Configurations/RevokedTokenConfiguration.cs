using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
    {
        public void Configure(EntityTypeBuilder<RevokedToken> builder)
        {
            builder.ToTable("revoked_tokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id");

            builder.Property(t => t.Jti)
                   .HasColumnName("jti")
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(t => t.ExpiresAt)
                   .HasColumnName("expires_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(t => t.RevokedAt)
                   .HasColumnName("revoked_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Unique index so a JTI can only be revoked once; also used for fast lookups
            builder.HasIndex(t => t.Jti)
                   .IsUnique()
                   .HasDatabaseName("ix_revoked_tokens_jti");

            // Index to support cleanup of expired revocations
            builder.HasIndex(t => t.ExpiresAt)
                   .HasDatabaseName("ix_revoked_tokens_expires_at");
        }
    }
}
