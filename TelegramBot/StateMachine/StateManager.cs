using System.Collections.Concurrent;

namespace TelegramBot.StateMachine;

/// <summary>
/// Manages per-user FSM state and temporary data (e.g., pending movie title).
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public class StateManager
{
    /// <summary>
    /// Stores (UserState, temporary data) tuples keyed by Telegram user ID.
    /// Temporary data holds the movie title while awaiting genre selection.
    /// </summary>
    private readonly ConcurrentDictionary<long, (UserState State, string? TempData)> _states = new();

    /// <summary>
    /// Gets the current state for a user. Returns None if no state is tracked.
    /// </summary>
    public UserState GetState(long userId)
    {
        return _states.TryGetValue(userId, out var entry) ? entry.State : UserState.None;
    }

    /// <summary>
    /// Gets the temporary data stored for a user (e.g., pending movie title).
    /// </summary>
    public string? GetTempData(long userId)
    {
        return _states.TryGetValue(userId, out var entry) ? entry.TempData : null;
    }

    /// <summary>
    /// Sets the state and optional temporary data for a user.
    /// </summary>
    public void SetState(long userId, UserState state, string? tempData = null)
    {
        _states[userId] = (state, tempData);
    }

    /// <summary>
    /// Resets a user's state to None and clears temporary data.
    /// </summary>
    public void ResetState(long userId)
    {
        _states[userId] = (UserState.None, null);
    }
}
