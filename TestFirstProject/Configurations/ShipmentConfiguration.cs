using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(s => s.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.TrackingNumber)
            .HasColumnName("tracking_number")
            .HasMaxLength(100);

        builder.Property(s => s.Carrier)
            .HasColumnName("carrier")
            .HasMaxLength(100);

        builder.Property(s => s.ShippingCost)
            .HasColumnName("shipping_cost")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.WeightKg)
            .HasColumnName("weight_kg")
            .HasPrecision(10, 3);

        builder.Property(s => s.DistanceZone)
            .HasColumnName("distance_zone")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ShippedAt)
            .HasColumnName("shipped_at");

        builder.Property(s => s.EstimatedDeliveryDate)
            .HasColumnName("estimated_delivery_date");

        builder.Property(s => s.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(s => s.Order)
            .WithOne(o => o.Shipment)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.OrderId).IsUnique();
        builder.HasIndex(s => s.TrackingNumber);
    }
}
