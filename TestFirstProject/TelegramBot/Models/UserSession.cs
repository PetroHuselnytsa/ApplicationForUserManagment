namespace TestFirstProject.TelegramBot.Models
{
    /// <summary>
    /// Holds per-user FSM state for the Telegram bot dialog.
    /// </summary>
    public class UserSession
    {
        /// <summary>Current dialog state.</summary>
        public BotState State { get; set; } = BotState.Idle;

        /// <summary>
        /// Temporarily holds the movie title entered by the user while waiting for a genre selection.
        /// </summary>
        public string? PendingTitle { get; set; }
    }
}
