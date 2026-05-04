namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book in the library.
    /// Supports soft delete via IsDeleted flag and DeletedAt timestamp.
    /// </summary>
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int Year { get; set; }
        public string Genre { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
