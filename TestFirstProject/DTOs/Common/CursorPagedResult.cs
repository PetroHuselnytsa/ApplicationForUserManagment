namespace TestFirstProject.DTOs.Common
{
    /// <summary>
    /// Cursor-based pagination result for efficient message history retrieval.
    /// The cursor is the ID of the last item; the client passes it back to get the next page.
    /// </summary>
    public class CursorPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
    }
}
