using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Location)
            .HasColumnName("location")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
    }
}
