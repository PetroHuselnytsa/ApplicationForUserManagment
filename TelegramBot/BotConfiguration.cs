namespace TelegramBot;

/// <summary>
/// Configuration model for Telegram bot settings. Bound from appsettings.json section "TelegramBot".
/// </summary>
public class BotConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "TelegramBot";

    /// <summary>
    /// The Telegram Bot API token obtained from @BotFather.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
