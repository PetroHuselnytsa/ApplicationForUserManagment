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

            builder.Property(b => b.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(b => b.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(b => b.IsDeleted)
                   .HasColumnName("is_deleted")
                   .HasDefaultValue(false);

            builder.Property(b => b.DeletedAt)
                   .HasColumnName("deleted_at");

            // Global query filter to exclude soft-deleted books
            builder.HasQueryFilter(b => !b.IsDeleted);

            // Index on Genre for efficient filtering
            builder.HasIndex(b => b.Genre);
        }
    }
}
