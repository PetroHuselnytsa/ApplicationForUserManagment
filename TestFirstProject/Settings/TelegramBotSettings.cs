namespace TestFirstProject.Settings
{
    /// <summary>
    /// Configuration for the Telegram bot.
    /// Bound from appsettings.json "TelegramBot" section.
    /// </summary>
    public class TelegramBotSettings
    {
        public const string SectionName = "TelegramBot";

        /// <summary>The bot API token obtained from @BotFather.</summary>
        public string Token { get; set; } = string.Empty;
    }
}
