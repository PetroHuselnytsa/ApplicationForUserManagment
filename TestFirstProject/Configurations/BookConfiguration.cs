using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// Fluent API configuration for the <see cref="Book"/> entity.
    /// Maps to the "books" table with snake_case column names.
    /// </summary>
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("books");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(b => b.Title)
                   .HasColumnName("title")
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(b => b.Author)
                   .HasColumnName("author")
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(b => b.Year)
                   .HasColumnName("year")
                   .IsRequired();

            builder.Property(b => b.Genre)
                   .HasColumnName("genre")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.IsDeleted)
                   .HasColumnName("is_deleted")
                   .HasDefaultValue(false);

            builder.Property(b => b.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(b => b.UpdatedAt)
                   .HasColumnName("updated_at");

            // Indexes for common query patterns
            builder.HasIndex(b => b.Genre);
            builder.HasIndex(b => b.IsDeleted);
            builder.HasIndex(b => new { b.Genre, b.IsDeleted });
        }
    }
}
