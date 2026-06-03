namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an author of books in the library.
    /// </summary>
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Biography { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}
