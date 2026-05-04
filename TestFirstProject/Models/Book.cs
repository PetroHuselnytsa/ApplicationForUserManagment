namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book in the library catalog.
    /// Uses a soft-delete pattern — deleted books are hidden from queries but not removed from the database.
    /// </summary>
    public class Book
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        /// <summary>Publication year (e.g. 1984, 2024).</summary>
        public int Year { get; set; }

        public string Genre { get; set; } = null!;

        /// <summary>Soft-delete flag. When true the book is excluded from all public queries.</summary>
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
