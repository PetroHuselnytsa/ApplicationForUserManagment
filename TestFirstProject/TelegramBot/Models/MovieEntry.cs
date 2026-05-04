namespace TestFirstProject.TelegramBot.Models
{
    /// <summary>
    /// Represents a single movie in a user's watchlist.
    /// </summary>
    public class MovieEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
    }
}
