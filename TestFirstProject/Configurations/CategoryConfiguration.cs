using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(500);
            builder.Property(c => c.ParentCategoryId).HasColumnName("parent_category_id");

            // Self-referencing hierarchy
            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.Name);

            // Seed data
            var electronics = new Category { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000001"), Name = "Electronics", Description = "Electronic devices and accessories" };
            var clothing = new Category { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000002"), Name = "Clothing", Description = "Apparel and fashion" };
            var phones = new Category { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000003"), Name = "Phones", Description = "Smartphones and accessories", ParentCategoryId = electronics.Id };
            var laptops = new Category { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000004"), Name = "Laptops", Description = "Notebooks and ultrabooks", ParentCategoryId = electronics.Id };
            var mensClothing = new Category { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000005"), Name = "Men's Clothing", Description = "Men's apparel", ParentCategoryId = clothing.Id };

            builder.HasData(electronics, clothing, phones, laptops, mensClothing);
        }
    }
}
