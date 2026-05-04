using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// ─────────────────────────────────────────────────────────────────────────────
// Telegram Bot – handles /hello and /sum <n1> <n2> commands.
// Bot token is read from the TELEGRAM_BOT_TOKEN environment variable.
// ─────────────────────────────────────────────────────────────────────────────

string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.Error.WriteLine(
        "ERROR: Environment variable TELEGRAM_BOT_TOKEN is not set. " +
        "Please set it before running the bot.");
    Environment.Exit(1);
}

var botClient = new TelegramBotClient(token);

// Verify the token and display bot info
User me = await botClient.GetMe();
Console.WriteLine($"Bot started: @{me.Username} (id={me.Id})");
Console.WriteLine("Listening for updates... Press Ctrl+C to stop.");

using var cts = new CancellationTokenSource();

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Only receive Message updates to reduce noise
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = [UpdateType.Message],
    DropPendingUpdates = true   // ignore messages sent while bot was offline
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandleErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token);

// Keep the process alive until cancelled
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Bot stopped.");
}

// ─────────────────────────────────────────────────────────────────────────────
// Update handler – dispatches incoming messages to command handlers
// ─────────────────────────────────────────────────────────────────────────────
async Task HandleUpdateAsync(
    ITelegramBotClient client,
    Update update,
    CancellationToken cancellationToken)
{
    // We only care about text messages
    if (update.Message is not { } message) return;
    if (message.Text is not { } text) return;

    long chatId = message.Chat.Id;
    string firstName = message.From?.FirstName ?? "stranger";

    Console.WriteLine($"[{DateTime.UtcNow:u}] {firstName} ({chatId}): {text}");

    // Normalize: strip bot-username suffix (e.g. /hello@MyBot → /hello)
    string command = text.Trim();
    int atIndex = command.IndexOf('@');
    if (atIndex > 0)
    {
        int spaceAfterAt = command.IndexOf(' ', atIndex);
        command = spaceAfterAt < 0
            ? command[..atIndex]
            : command[..atIndex] + command[spaceAfterAt..];
    }

    if (command.StartsWith("/hello", StringComparison.OrdinalIgnoreCase))
    {
        await HandleHelloAsync(client, chatId, firstName, cancellationToken);
    }
    else if (command.StartsWith("/sum", StringComparison.OrdinalIgnoreCase))
    {
        await HandleSumAsync(client, chatId, command, cancellationToken);
    }
    else if (command.StartsWith("/"))
    {
        // Unknown command
        await client.SendMessage(
            chatId: chatId,
            text: "Unknown command. Available commands:\n" +
                  "  /hello – get a personalized greeting\n" +
                  "  /sum <n1> <n2> – calculate the sum of two numbers",
            cancellationToken: cancellationToken);
    }
    // Non-command messages are silently ignored
}

// ─────────────────────────────────────────────────────────────────────────────
// /hello handler
// ─────────────────────────────────────────────────────────────────────────────
async Task HandleHelloAsync(
    ITelegramBotClient client,
    long chatId,
    string firstName,
    CancellationToken cancellationToken)
{
    await client.SendMessage(
        chatId: chatId,
        text: $"Hello, {firstName}!",
        cancellationToken: cancellationToken);
}

// ─────────────────────────────────────────────────────────────────────────────
// /sum <n1> <n2> handler
// ─────────────────────────────────────────────────────────────────────────────
async Task HandleSumAsync(
    ITelegramBotClient client,
    long chatId,
    string fullCommand,
    CancellationToken cancellationToken)
{
    // Expected format: /sum <n1> <n2>
    string[] parts = fullCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length != 3
        || !double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double n1)
        || !double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double n2))
    {
        await client.SendMessage(
            chatId: chatId,
            text: "Usage: /sum <n1> <n2>\nExample: /sum 3 5",
            cancellationToken: cancellationToken);
        return;
    }

    double result = n1 + n2;

    // Format as integer when the result is a whole number, otherwise show decimals
    string resultText = result == Math.Floor(result)
        ? result.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
        : result.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

    await client.SendMessage(
        chatId: chatId,
        text: $"The sum of {parts[1]} and {parts[2]} is {resultText}",
        cancellationToken: cancellationToken);
}

// ─────────────────────────────────────────────────────────────────────────────
// Error handler – logs Telegram API / polling errors without crashing
// ─────────────────────────────────────────────────────────────────────────────
Task HandleErrorAsync(
    ITelegramBotClient client,
    Exception exception,
    HandleErrorSource source,
    CancellationToken cancellationToken)
{
    string errorMessage = exception switch
    {
        ApiRequestException apiEx =>
            $"Telegram API error [{apiEx.ErrorCode}]: {apiEx.Message}",
        _ => exception.ToString()
    };

    Console.Error.WriteLine($"[{DateTime.UtcNow:u}] ERROR ({source}): {errorMessage}");
    return Task.CompletedTask;
}
