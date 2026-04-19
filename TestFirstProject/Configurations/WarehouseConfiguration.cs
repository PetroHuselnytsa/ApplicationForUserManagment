using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("warehouses");

            builder.HasKey(w => w.Id);
            builder.Property(w => w.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(w => w.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(w => w.Location).HasColumnName("location").IsRequired().HasMaxLength(200);
            builder.Property(w => w.IsActive).HasColumnName("is_active");

            builder.HasIndex(w => w.Name).IsUnique();

            // Seed data
            builder.HasData(
                new Warehouse { Id = Guid.Parse("d1d2d3d4-0004-0004-0004-000000000001"), Name = "Main Warehouse", Location = "New York, NY" },
                new Warehouse { Id = Guid.Parse("d1d2d3d4-0004-0004-0004-000000000002"), Name = "West Coast Hub", Location = "Los Angeles, CA" },
                new Warehouse { Id = Guid.Parse("d1d2d3d4-0004-0004-0004-000000000003"), Name = "Central Distribution", Location = "Chicago, IL" }
            );
        }
    }
}
