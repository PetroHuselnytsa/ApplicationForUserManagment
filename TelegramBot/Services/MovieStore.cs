using System.Collections.Concurrent;
using TelegramBot.Models;

namespace TelegramBot.Services;

/// <summary>
/// In-memory movie watchlist storage, keyed by Telegram user ID.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public class MovieStore
{
    private readonly ConcurrentDictionary<long, List<Movie>> _movies = new();
    private readonly object _lock = new();

    /// <summary>
    /// Adds a movie to the user's watchlist.
    /// </summary>
    public void AddMovie(long userId, Movie movie)
    {
        _movies.AddOrUpdate(
            userId,
            _ => new List<Movie> { movie },
            (_, list) =>
            {
                lock (_lock)
                {
                    list.Add(movie);
                }
                return list;
            });
    }

    /// <summary>
    /// Gets all movies for a user. Returns an empty list if none exist.
    /// </summary>
    public List<Movie> GetMovies(long userId)
    {
        if (_movies.TryGetValue(userId, out var list))
        {
            lock (_lock)
            {
                return new List<Movie>(list);
            }
        }
        return new List<Movie>();
    }

    /// <summary>
    /// Clears all movies from a user's watchlist.
    /// </summary>
    public void ClearMovies(long userId)
    {
        _movies.TryRemove(userId, out _);
    }

    /// <summary>
    /// Returns true if the user has at least one movie in their watchlist.
    /// </summary>
    public bool HasMovies(long userId)
    {
        return _movies.TryGetValue(userId, out var list) && list.Count > 0;
    }
}
