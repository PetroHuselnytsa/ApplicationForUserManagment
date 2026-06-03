using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
    {
        public void Configure(EntityTypeBuilder<BookAuthor> builder)
        {
            builder.ToTable("book_authors");

            builder.HasKey(ba => new { ba.BookId, ba.AuthorId });

            builder.Property(ba => ba.BookId)
                .HasColumnName("book_id");

            builder.Property(ba => ba.AuthorId)
                .HasColumnName("author_id");

            // Relationships
            builder.HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for author lookup
            builder.HasIndex(ba => ba.AuthorId);
        }
    }
}
