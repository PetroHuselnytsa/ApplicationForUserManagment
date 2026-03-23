using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AuditLog entity.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Timestamp)
               .HasColumnName("timestamp")
               .HasColumnType("timestamp with time zone")
               .IsRequired()
               .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UserId)
               .HasColumnName("user_id")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(a => a.UserName)
               .HasColumnName("user_name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(a => a.Action)
               .HasColumnName("action")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(a => a.EntityType)
               .HasColumnName("entity_type")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(a => a.EntityId)
               .HasColumnName("entity_id")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(a => a.OldValues)
               .HasColumnName("old_values")
               .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
               .HasColumnName("new_values")
               .HasColumnType("jsonb");

        builder.Property(a => a.AdditionalData)
               .HasColumnName("additional_data")
               .HasColumnType("jsonb");

        builder.Property(a => a.IpAddress)
               .HasColumnName("ip_address")
               .HasMaxLength(45)
               .IsRequired();

        builder.Property(a => a.UserAgent)
               .HasColumnName("user_agent")
               .HasColumnType("text");

        builder.Property(a => a.CorrelationId)
               .HasColumnName("correlation_id")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(a => a.RequestPath)
               .HasColumnName("request_path")
               .HasMaxLength(2048);

        builder.Property(a => a.RequestMethod)
               .HasColumnName("request_method")
               .HasMaxLength(10);

        builder.Property(a => a.ResponseStatusCode)
               .HasColumnName("response_status_code");

        builder.Property(a => a.Environment)
               .HasColumnName("environment")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(a => a.Checksum)
               .HasColumnName("checksum")
               .HasMaxLength(64)
               .IsRequired();

        // Indexes for common query patterns
        builder.HasIndex(a => a.Timestamp)
               .HasDatabaseName("idx_audit_logs_timestamp")
               .IsDescending();

        builder.HasIndex(a => a.UserId)
               .HasDatabaseName("idx_audit_logs_user_id");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
               .HasDatabaseName("idx_audit_logs_entity");

        builder.HasIndex(a => a.CorrelationId)
               .HasDatabaseName("idx_audit_logs_correlation_id");

        builder.HasIndex(a => a.Action)
               .HasDatabaseName("idx_audit_logs_action");
    }
}
