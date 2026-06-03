using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book loan/checkout in the library system.
    /// </summary>
    public class Loan
    {
        public Guid Id { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;

        public Guid BorrowerId { get; set; }
        public AppUser Borrower { get; set; } = null!;

        public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Active;
        public string? Notes { get; set; }
    }
}
