using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
    {
        public void Configure(EntityTypeBuilder<ShippingZone> builder)
        {
            builder.ToTable("shipping_zones");

            builder.HasKey(z => z.Id);
            builder.Property(z => z.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(z => z.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(z => z.BaseCost).HasColumnName("base_cost").HasColumnType("decimal(18,2)");
            builder.Property(z => z.CostPerKg).HasColumnName("cost_per_kg").HasColumnType("decimal(18,2)");
            builder.Property(z => z.ExpressMultiplier).HasColumnName("express_multiplier").HasColumnType("decimal(5,2)");
            builder.Property(z => z.SameDayMultiplier).HasColumnName("same_day_multiplier").HasColumnType("decimal(5,2)");
            builder.Property(z => z.StandardDeliveryDays).HasColumnName("standard_delivery_days");
            builder.Property(z => z.ExpressDeliveryDays).HasColumnName("express_delivery_days");

            builder.HasIndex(z => z.Name).IsUnique();

            // Seed shipping zones
            builder.HasData(
                new ShippingZone { Id = Guid.Parse("f1f2f3f4-0006-0006-0006-000000000001"), Name = "Local", BaseCost = 5.00m, CostPerKg = 1.00m, ExpressMultiplier = 2.0m, SameDayMultiplier = 3.0m, StandardDeliveryDays = 3, ExpressDeliveryDays = 1 },
                new ShippingZone { Id = Guid.Parse("f1f2f3f4-0006-0006-0006-000000000002"), Name = "Regional", BaseCost = 10.00m, CostPerKg = 2.00m, ExpressMultiplier = 2.0m, SameDayMultiplier = 3.5m, StandardDeliveryDays = 5, ExpressDeliveryDays = 2 },
                new ShippingZone { Id = Guid.Parse("f1f2f3f4-0006-0006-0006-000000000003"), Name = "National", BaseCost = 15.00m, CostPerKg = 3.00m, ExpressMultiplier = 2.5m, SameDayMultiplier = 4.0m, StandardDeliveryDays = 7, ExpressDeliveryDays = 3 }
            );
        }
    }
}
