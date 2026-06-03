namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a book category/genre classification.
    /// </summary>
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    }
}
