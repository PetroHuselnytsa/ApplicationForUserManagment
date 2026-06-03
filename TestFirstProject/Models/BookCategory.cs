namespace TestFirstProject.Models
{
    /// <summary>
    /// Join entity for Book-Category many-to-many relationship.
    /// </summary>
    public class BookCategory
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
