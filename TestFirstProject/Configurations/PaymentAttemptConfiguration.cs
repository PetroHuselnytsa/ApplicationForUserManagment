using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(a => a.IsSuccessful)
            .HasColumnName("is_successful")
            .IsRequired();

        builder.Property(a => a.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(a => a.GatewayResponse)
            .HasColumnName("gateway_response")
            .HasMaxLength(2000);

        builder.Property(a => a.AttemptedAt)
            .HasColumnName("attempted_at")
            .IsRequired();

        builder.HasOne(a => a.Payment)
            .WithMany(p => p.Attempts)
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.PaymentId);
    }
}
