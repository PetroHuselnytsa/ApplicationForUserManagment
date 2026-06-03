namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book in the library catalog.
    /// </summary>
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Isbn { get; set; } = null!;
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string? Genre { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int? PageCount { get; set; }
        public string? Language { get; set; }
        public string? Edition { get; set; }
        public int TotalCopies { get; set; } = 1;
        public int AvailableCopies { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
