using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
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

            builder.Property(b => b.Isbn)
                .HasColumnName("isbn")
                .IsRequired()
                .HasMaxLength(13);

            builder.Property(b => b.Publisher)
                .HasColumnName("publisher")
                .HasMaxLength(255);

            builder.Property(b => b.PublicationYear)
                .HasColumnName("publication_year");

            builder.Property(b => b.Genre)
                .HasColumnName("genre")
                .HasMaxLength(100);

            builder.Property(b => b.Description)
                .HasColumnName("description")
                .HasMaxLength(4000);

            builder.Property(b => b.CoverImageUrl)
                .HasColumnName("cover_image_url")
                .HasMaxLength(2000);

            builder.Property(b => b.PageCount)
                .HasColumnName("page_count");

            builder.Property(b => b.Language)
                .HasColumnName("language")
                .HasMaxLength(50);

            builder.Property(b => b.Edition)
                .HasColumnName("edition")
                .HasMaxLength(100);

            builder.Property(b => b.TotalCopies)
                .HasColumnName("total_copies")
                .HasDefaultValue(1);

            builder.Property(b => b.AvailableCopies)
                .HasColumnName("available_copies")
                .HasDefaultValue(1);

            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(b => b.UpdatedAt)
                .HasColumnName("updated_at");

            // Indexes
            builder.HasIndex(b => b.Isbn).IsUnique();
            builder.HasIndex(b => b.Title);
            builder.HasIndex(b => b.Genre);
            builder.HasIndex(b => b.PublicationYear);
        }
    }
}
