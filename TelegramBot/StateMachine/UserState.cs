namespace TelegramBot.StateMachine;

/// <summary>
/// FSM states for the bot dialog flow.
/// </summary>
public enum UserState
{
    /// <summary>
    /// Default state — no active dialog.
    /// </summary>
    None,

    /// <summary>
    /// Bot is waiting for the user to type a movie title.
    /// </summary>
    AwaitingMovieTitle,

    /// <summary>
    /// Bot is waiting for the user to select a genre via inline keyboard.
    /// </summary>
    AwaitingMovieGenre,

    /// <summary>
    /// Bot is waiting for the user to confirm clearing the watchlist.
    /// </summary>
    AwaitingClearConfirmation
}
