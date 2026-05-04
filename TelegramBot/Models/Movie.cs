namespace TelegramBot.Models;

/// <summary>
/// Represents a movie in the user's watchlist.
/// </summary>
public class Movie
{
    /// <summary>
    /// The title of the movie.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The genre of the movie (Action, Comedy, Drama, Sci-Fi).
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    public override string ToString() => $"{Title} [{Genre}]";
}
