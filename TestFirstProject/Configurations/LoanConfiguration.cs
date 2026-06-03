using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("loans");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(l => l.BookId)
                .HasColumnName("book_id");

            builder.Property(l => l.BorrowerId)
                .HasColumnName("borrower_id");

            builder.Property(l => l.CheckoutDate)
                .HasColumnName("checkout_date");

            builder.Property(l => l.DueDate)
                .HasColumnName("due_date");

            builder.Property(l => l.ReturnDate)
                .HasColumnName("return_date");

            builder.Property(l => l.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(l => l.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            // Relationships
            builder.HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Borrower)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(l => l.BookId);
            builder.HasIndex(l => l.Status);
            builder.HasIndex(l => l.DueDate);
            // Composite index on {BorrowerId, Status} - also covers BorrowerId-only queries
            builder.HasIndex(l => new { l.BorrowerId, l.Status });
        }
    }
}
