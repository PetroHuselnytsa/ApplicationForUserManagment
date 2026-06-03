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

            builder.Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at");

            // Indexes
            builder.HasIndex(c => c.Name).IsUnique();
        }
    }
}
