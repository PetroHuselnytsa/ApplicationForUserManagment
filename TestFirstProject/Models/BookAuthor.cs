namespace TestFirstProject.Models
{
    /// <summary>
    /// Join entity for Book-Author many-to-many relationship.
    /// </summary>
    public class BookAuthor
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }
}
