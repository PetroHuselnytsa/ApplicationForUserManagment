using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(h => h.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(h => h.FromStatus)
            .HasColumnName("from_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.ToStatus)
            .HasColumnName("to_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.ChangedByUserId)
            .HasColumnName("changed_by_user_id")
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(h => h.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired();

        builder.HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.OrderId);
        builder.HasIndex(h => h.ChangedAt);
    }
}
