using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TestFirstProject.TelegramBot.Models;

namespace TestFirstProject.TelegramBot
{
    /// <summary>
    /// Handles all incoming Telegram updates (messages and callback queries).
    /// Implements a simple FSM to drive the Add Movie, My List, and Random Pick flows.
    /// </summary>
    public class BotUpdateHandler : IUpdateHandler
    {
        private readonly WatchlistStorage _storage;
        private readonly ILogger<BotUpdateHandler> _logger;

        // Supported genres exposed as inline keyboard options.
        private static readonly string[] Genres = { "Action", "Comedy", "Drama", "Sci-Fi" };

        public BotUpdateHandler(WatchlistStorage storage, ILogger<BotUpdateHandler> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        // ── IUpdateHandler ────────────────────────────────────────────────────

        public async Task HandleUpdateAsync(
            ITelegramBotClient bot,
            Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message is { } message)
                {
                    await HandleMessageAsync(bot, message, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } callbackQuery)
                {
                    await HandleCallbackQueryAsync(bot, callbackQuery, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing update {UpdateId}", update.Id);
            }
        }

        public Task HandleErrorAsync(
            ITelegramBotClient bot,
            Exception exception,
            HandleErrorSource source,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Telegram polling error from source {Source}", source);
            return Task.CompletedTask;
        }

        // ── Message handling ──────────────────────────────────────────────────

        private async Task HandleMessageAsync(
            ITelegramBotClient bot,
            Message message,
            CancellationToken ct)
        {
            // Ignore messages without text (stickers, photos, etc.)
            if (message.Text is not { } text)
                return;

            long userId = message.From?.Id ?? 0;
            long chatId = message.Chat.Id;

            if (userId == 0)
                return;

            var session = _storage.GetSession(userId);

            // Commands always take priority regardless of current FSM state.
            switch (text.Trim())
            {
                case "/start":
                    _storage.ResetSession(userId);
                    await SendMainMenuAsync(bot, chatId, "Welcome to your Movie Watchlist! Choose an option:", ct);
                    return;

                case "/cancel":
                    _storage.ResetSession(userId);
                    await bot.SendMessage(chatId, "Action cancelled. Returning to main menu.", cancellationToken: ct);
                    await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
                    return;

                case "/clear":
                    session.State = BotState.AwaitingClearConfirm;
                    await bot.SendMessage(
                        chatId,
                        "Are you sure you want to wipe your entire watchlist?",
                        replyMarkup: BuildClearConfirmKeyboard(),
                        cancellationToken: ct);
                    return;
            }

            // Non-command text — handle based on current FSM state.
            switch (session.State)
            {
                case BotState.AwaitingTitle:
                    // User typed the movie title; now ask for a genre.
                    string title = text.Trim();
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        await bot.SendMessage(chatId, "Title cannot be empty. Please enter a movie title:", cancellationToken: ct);
                        return;
                    }

                    session.PendingTitle = title;
                    session.State = BotState.AwaitingGenre;
                    await bot.SendMessage(
                        chatId,
                        $"Got it! Now pick a genre for \"{title}\":",
                        replyMarkup: BuildGenreKeyboard(),
                        cancellationToken: ct);
                    break;

                case BotState.Idle:
                case BotState.AwaitingGenre:
                case BotState.AwaitingClearConfirm:
                default:
                    // Unexpected free text — nudge the user back to the menu.
                    await SendMainMenuAsync(bot, chatId, "Use the menu to navigate:", ct);
                    break;
            }
        }

        // ── Callback query handling ───────────────────────────────────────────

        private async Task HandleCallbackQueryAsync(
            ITelegramBotClient bot,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            long userId = callbackQuery.From.Id;
            long chatId = callbackQuery.Message?.Chat.Id ?? 0;
            string data = callbackQuery.Data ?? string.Empty;

            if (chatId == 0)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            var session = _storage.GetSession(userId);

            // Always acknowledge the callback to remove Telegram's loading indicator.
            await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

            switch (data)
            {
                // ── Main menu ────────────────────────────────────────────────
                case "menu_add":
                    _storage.ResetSession(userId);
                    session = _storage.GetSession(userId);
                    session.State = BotState.AwaitingTitle;
                    await bot.SendMessage(chatId, "Enter the movie title:", cancellationToken: ct);
                    break;

                case "menu_list":
                    _storage.ResetSession(userId);
                    await SendMovieListAsync(bot, chatId, userId, ct);
                    break;

                case "menu_random":
                    _storage.ResetSession(userId);
                    await SendRandomPickAsync(bot, chatId, userId, ct);
                    break;

                // ── Genre selection ──────────────────────────────────────────
                case var genre when data.StartsWith("genre_") && session.State == BotState.AwaitingGenre:
                {
                    string genreName = genre["genre_".Length..];
                    string pendingTitle = session.PendingTitle ?? "Unknown";

                    _storage.AddMovie(userId, new MovieEntry { Title = pendingTitle, Genre = genreName });
                    _storage.ResetSession(userId);

                    await bot.SendMessage(
                        chatId,
                        $"Saved: \"{pendingTitle}\" ({genreName}).",
                        cancellationToken: ct);
                    await SendMainMenuAsync(bot, chatId, "What would you like to do next?", ct);
                    break;
                }

                // ── Clear confirmation ────────────────────────────────────────
                case "clear_yes" when session.State == BotState.AwaitingClearConfirm:
                    _storage.ClearMovies(userId);
                    _storage.ResetSession(userId);
                    await bot.SendMessage(chatId, "Your watchlist has been cleared.", cancellationToken: ct);
                    await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
                    break;

                case "clear_no" when session.State == BotState.AwaitingClearConfirm:
                    _storage.ResetSession(userId);
                    await bot.SendMessage(chatId, "Clear cancelled.", cancellationToken: ct);
                    await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
                    break;

                default:
                    // Stale or unexpected callback — show the main menu.
                    _storage.ResetSession(userId);
                    await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
                    break;
            }
        }

        // ── Helper methods ────────────────────────────────────────────────────

        private static Task SendMainMenuAsync(
            ITelegramBotClient bot,
            long chatId,
            string prompt,
            CancellationToken ct)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Add Movie", "menu_add"),
                    InlineKeyboardButton.WithCallbackData("My List",   "menu_list"),
                    InlineKeyboardButton.WithCallbackData("Random Pick", "menu_random")
                }
            });

            return bot.SendMessage(chatId, prompt, replyMarkup: keyboard, cancellationToken: ct);
        }

        private static InlineKeyboardMarkup BuildGenreKeyboard()
        {
            // Two genres per row.
            var buttons = Genres
                .Select(g => InlineKeyboardButton.WithCallbackData(g, $"genre_{g}"))
                .Chunk(2)
                .Select(row => row.ToArray())
                .ToArray();

            return new InlineKeyboardMarkup(buttons);
        }

        private static InlineKeyboardMarkup BuildClearConfirmKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Yes, clear it", "clear_yes"),
                    InlineKeyboardButton.WithCallbackData("No, keep it",   "clear_no")
                }
            });
        }

        private async Task SendMovieListAsync(
            ITelegramBotClient bot,
            long chatId,
            long userId,
            CancellationToken ct)
        {
            var movies = _storage.GetMovies(userId);

            if (movies.Count == 0)
            {
                await bot.SendMessage(chatId, "Your list is empty.", cancellationToken: ct);
                await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
                return;
            }

            // Snapshot the list while it may be concurrently modified.
            List<MovieEntry> snapshot;
            lock (movies)
            {
                snapshot = movies.ToList();
            }

            var lines = snapshot
                .Select((m, i) => $"{i + 1}. {m.Title} ({m.Genre})")
                .ToList();

            await bot.SendMessage(chatId, string.Join("\n", lines), cancellationToken: ct);
            await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
        }

        private async Task SendRandomPickAsync(
            ITelegramBotClient bot,
            long chatId,
            long userId,
            CancellationToken ct)
        {
            var movies = _storage.GetMovies(userId);

            MovieEntry? pick;
            lock (movies)
            {
                pick = movies.Count == 0 ? null : movies[Random.Shared.Next(movies.Count)];
            }

            if (pick is null)
            {
                await bot.SendMessage(chatId, "Your list is empty.", cancellationToken: ct);
            }
            else
            {
                await bot.SendMessage(chatId, $"Random pick: {pick.Title} ({pick.Genre})", cancellationToken: ct);
            }

            await SendMainMenuAsync(bot, chatId, "Main menu:", ct);
        }
    }
}
