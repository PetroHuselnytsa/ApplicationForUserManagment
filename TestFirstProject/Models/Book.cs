namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book in the library.
    /// </summary>
    public class Book
    {
        /// <summary>Primary key — auto-generated GUID.</summary>
        public Guid Id { get; set; }

        /// <summary>Title of the book.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Author of the book.</summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>Publication year.</summary>
        public int Year { get; set; }

        /// <summary>Genre / category of the book (free text).</summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>Soft-delete flag — when true the record is hidden from normal queries.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>UTC timestamp when the record was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last updated.</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
