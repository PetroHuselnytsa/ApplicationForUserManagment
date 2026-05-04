using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// Fluent API configuration for the <see cref="Book"/> entity.
    /// Follows the existing project convention: snake_case table and column names,
    /// no data annotations on the model class.
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
                   .HasMaxLength(200);

            builder.Property(b => b.Author)
                   .HasColumnName("author")
                   .IsRequired()
                   .HasMaxLength(200);

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

            // Speed up genre-filtered listing queries
            builder.HasIndex(b => b.Genre)
                   .HasDatabaseName("ix_books_genre");

            // Speed up soft-delete filter applied to every read
            builder.HasIndex(b => b.IsDeleted)
                   .HasDatabaseName("ix_books_is_deleted");
        }
    }
}
