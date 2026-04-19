using System.Collections.Concurrent;
using TestFirstProject.Exceptions;

namespace TestFirstProject.Services
{
    /// <summary>
    /// In-memory rate limiter that enforces max 30 messages per minute per user.
    /// Uses a sliding window approach with a ConcurrentDictionary.
    /// </summary>
    public interface IMessageRateLimiter
    {
        /// <summary>
        /// Check and record a message send attempt. Throws RateLimitExceededException if over limit.
        /// </summary>
        void ValidateAndRecord(Guid userId);
    }

    public class MessageRateLimiter : IMessageRateLimiter
    {
        private const int MaxMessagesPerMinute = 30;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        // Maps userId -> list of send timestamps within the current window
        private readonly ConcurrentDictionary<Guid, List<DateTime>> _timestamps = new();

        public void ValidateAndRecord(Guid userId)
        {
            var now = DateTime.UtcNow;
            var cutoff = now - Window;

            var timestamps = _timestamps.GetOrAdd(userId, _ => new List<DateTime>());

            lock (timestamps)
            {
                // Remove expired timestamps
                timestamps.RemoveAll(t => t < cutoff);

                if (timestamps.Count >= MaxMessagesPerMinute)
                    throw new RateLimitExceededException();

                timestamps.Add(now);
            }
        }
    }
}
