namespace TestFirstProject.TelegramBot.Models
{
    /// <summary>
    /// FSM states for the per-user Telegram bot dialog.
    /// </summary>
    public enum BotState
    {
        /// <summary>No active dialog — user is at the main menu.</summary>
        Idle,

        /// <summary>Bot has asked the user to type a movie title.</summary>
        AwaitingTitle,

        /// <summary>Bot has asked the user to pick a genre via inline keyboard.</summary>
        AwaitingGenre,

        /// <summary>Bot has asked the user to confirm clearing their watchlist.</summary>
        AwaitingClearConfirm
    }
}
