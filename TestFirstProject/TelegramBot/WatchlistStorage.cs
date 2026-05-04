using System.Collections.Concurrent;
using TestFirstProject.TelegramBot.Models;

namespace TestFirstProject.TelegramBot
{
    /// <summary>
    /// Thread-safe in-memory store for per-user movie watchlists and dialog sessions.
    /// Registered as a singleton so all components share the same state.
    /// </summary>
    public class WatchlistStorage
    {
        // Keyed by Telegram user ID.
        private readonly ConcurrentDictionary<long, List<MovieEntry>> _watchlists = new();
        private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

        // Each user's list gets its own dedicated lock to minimise contention.
        private readonly ConcurrentDictionary<long, object> _listLocks = new();

        // ── Watchlist operations ──────────────────────────────────────────────

        /// <summary>Returns the list for a user, creating it lazily if absent.</summary>
        public List<MovieEntry> GetMovies(long userId)
        {
            return _watchlists.GetOrAdd(userId, _ => new List<MovieEntry>());
        }

        /// <summary>Appends a movie to the user's watchlist under a per-user lock.</summary>
        public void AddMovie(long userId, MovieEntry movie)
        {
            var lockObj = _listLocks.GetOrAdd(userId, _ => new object());
            lock (lockObj)
            {
                var list = _watchlists.GetOrAdd(userId, _ => new List<MovieEntry>());
                list.Add(movie);
            }
        }

        /// <summary>Removes all movies from the user's watchlist under a per-user lock.</summary>
        public void ClearMovies(long userId)
        {
            var lockObj = _listLocks.GetOrAdd(userId, _ => new object());
            lock (lockObj)
            {
                if (_watchlists.TryGetValue(userId, out var list))
                    list.Clear();
            }
        }

        // ── Session / FSM state operations ───────────────────────────────────

        /// <summary>Returns the session for a user, creating a default one if absent.</summary>
        public UserSession GetSession(long userId)
        {
            return _sessions.GetOrAdd(userId, _ => new UserSession());
        }

        /// <summary>Resets the user's dialog state to Idle and clears any pending data.</summary>
        public void ResetSession(long userId)
        {
            var session = _sessions.GetOrAdd(userId, _ => new UserSession());
            session.State = BotState.Idle;
            session.PendingTitle = null;
        }
    }
}
