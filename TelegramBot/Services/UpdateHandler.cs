using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.StateMachine;

namespace TelegramBot.Services;

/// <summary>
/// Handles all incoming Telegram updates: commands, callback queries, and text messages.
/// Routes updates based on FSM state and message type.
/// </summary>
public class UpdateHandler(
    ITelegramBotClient bot,
    ILogger<UpdateHandler> logger,
    MovieStore movieStore,
    StateManager stateManager) : IUpdateHandler
{
    private static readonly Random Random = new();

    /// <summary>
    /// Main entry point for processing an update from Telegram.
    /// Routes to the appropriate handler based on update type.
    /// </summary>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await (update switch
        {
            { Message: { } message } => OnMessage(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery, cancellationToken),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Handles errors during update processing. Logs the error and applies
    /// a cooldown for API request exceptions to avoid hammering the server.
    /// </summary>
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error handling update from source {Source}", source);

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    /// <summary>
    /// Handles incoming text messages. Dispatches commands (/start, /cancel, /clear)
    /// or processes text input based on the current FSM state.
    /// </summary>
    private async Task OnMessage(Message message, CancellationToken ct)
    {
        if (message.Text is not { } text)
            return;

        var userId = message.From!.Id;
        var chatId = message.Chat.Id;

        logger.LogInformation("Received message from user {UserId}: {Text}", userId, text);

        // Extract command (first word before @botname or space)
        var command = text.Split(' ', '@')[0].ToLowerInvariant();

        switch (command)
        {
            case "/start":
                stateManager.ResetState(userId);
                await SendMainMenu(chatId, "Welcome to Movie Watchlist Bot! 🎬\nChoose an option:", ct);
                return;

            case "/cancel":
                stateManager.ResetState(userId);
                await bot.SendMessage(chatId, "Action cancelled. Use /start to see the main menu.", cancellationToken: ct);
                return;

            case "/clear":
                stateManager.SetState(userId, UserState.AwaitingClearConfirmation);
                await SendClearConfirmation(chatId, ct);
                return;
        }

        // Handle text input based on current FSM state
        var state = stateManager.GetState(userId);
        switch (state)
        {
            case UserState.AwaitingMovieTitle:
                await HandleMovieTitleInput(userId, chatId, text, ct);
                break;

            case UserState.AwaitingMovieGenre:
                // User typed text instead of pressing an inline button — remind them
                await bot.SendMessage(chatId, "Please select a genre using the buttons above, or use /cancel to abort.", cancellationToken: ct);
                break;

            case UserState.AwaitingClearConfirmation:
                // User typed text instead of pressing Yes/No — remind them
                await bot.SendMessage(chatId, "Please press Yes or No above, or use /cancel to abort.", cancellationToken: ct);
                break;

            default:
                // No active dialog — show the main menu
                await SendMainMenu(chatId, "Use /start to see the main menu, or choose an option:", ct);
                break;
        }
    }

    /// <summary>
    /// Handles inline keyboard button presses (callback queries).
    /// Routes based on callback data to the appropriate feature handler.
    /// </summary>
    private async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken ct)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data ?? string.Empty;

        logger.LogInformation("Received callback from user {UserId}: {Data}", userId, data);

        // Acknowledge the callback to remove the loading indicator
        await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

        switch (data)
        {
            case "add_movie":
                stateManager.SetState(userId, UserState.AwaitingMovieTitle);
                await bot.SendMessage(chatId, "Enter the movie title:", cancellationToken: ct);
                break;

            case "my_list":
                await HandleMyList(userId, chatId, ct);
                break;

            case "random_pick":
                await HandleRandomPick(userId, chatId, ct);
                break;

            case "genre_action":
            case "genre_comedy":
            case "genre_drama":
            case "genre_scifi":
                await HandleGenreSelection(userId, chatId, data, ct);
                break;

            case "clear_yes":
                await HandleClearConfirm(userId, chatId, ct);
                break;

            case "clear_no":
                stateManager.ResetState(userId);
                await bot.SendMessage(chatId, "Clear cancelled. Your watchlist is safe! ✅", cancellationToken: ct);
                break;

            default:
                logger.LogWarning("Unknown callback data: {Data}", data);
                break;
        }
    }

    /// <summary>
    /// Stores the movie title in temp data and prompts the user to select a genre.
    /// Transitions FSM to AwaitingMovieGenre.
    /// </summary>
    private async Task HandleMovieTitleInput(long userId, long chatId, string title, CancellationToken ct)
    {
        stateManager.SetState(userId, UserState.AwaitingMovieGenre, title.Trim());

        var genreKeyboard = new InlineKeyboardMarkup()
            .AddNewRow()
                .AddButton("Action", "genre_action")
                .AddButton("Comedy", "genre_comedy")
            .AddNewRow()
                .AddButton("Drama", "genre_drama")
                .AddButton("Sci-Fi", "genre_scifi");

        await bot.SendMessage(chatId, $"Select a genre for \"{title.Trim()}\":", replyMarkup: genreKeyboard, cancellationToken: ct);
    }

    /// <summary>
    /// Completes the Add Movie flow: saves the movie with the selected genre
    /// and confirms to the user. Resets FSM to None.
    /// </summary>
    private async Task HandleGenreSelection(long userId, long chatId, string callbackData, CancellationToken ct)
    {
        var currentState = stateManager.GetState(userId);
        if (currentState != UserState.AwaitingMovieGenre)
        {
            await bot.SendMessage(chatId, "No movie is being added. Use /start to begin.", cancellationToken: ct);
            return;
        }

        var title = stateManager.GetTempData(userId);
        if (string.IsNullOrEmpty(title))
        {
            stateManager.ResetState(userId);
            await bot.SendMessage(chatId, "Something went wrong. Please try again with /start.", cancellationToken: ct);
            return;
        }

        var genre = callbackData switch
        {
            "genre_action" => "Action",
            "genre_comedy" => "Comedy",
            "genre_drama" => "Drama",
            "genre_scifi" => "Sci-Fi",
            _ => "Unknown"
        };

        var movie = new Movie { Title = title, Genre = genre };
        movieStore.AddMovie(userId, movie);
        stateManager.ResetState(userId);

        await bot.SendMessage(chatId, $"✅ Added \"{title}\" [{genre}] to your watchlist!", cancellationToken: ct);
    }

    /// <summary>
    /// Displays all movies in the user's watchlist, or an empty message if none exist.
    /// </summary>
    private async Task HandleMyList(long userId, long chatId, CancellationToken ct)
    {
        var movies = movieStore.GetMovies(userId);

        if (movies.Count == 0)
        {
            await bot.SendMessage(chatId, "Your list is empty.", cancellationToken: ct);
            return;
        }

        var sb = new StringBuilder("🎬 Your watchlist:\n\n");
        for (int i = 0; i < movies.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {movies[i].Title} [{movies[i].Genre}]");
        }

        await bot.SendMessage(chatId, sb.ToString(), cancellationToken: ct);
    }

    /// <summary>
    /// Picks a random movie from the user's watchlist and displays it.
    /// Shows an empty message if the list has no movies.
    /// </summary>
    private async Task HandleRandomPick(long userId, long chatId, CancellationToken ct)
    {
        var movies = movieStore.GetMovies(userId);

        if (movies.Count == 0)
        {
            await bot.SendMessage(chatId, "Your list is empty. Add some movies first!", cancellationToken: ct);
            return;
        }

        var pick = movies[Random.Next(movies.Count)];
        await bot.SendMessage(chatId, $"🎲 Random pick: {pick.Title} [{pick.Genre}]", cancellationToken: ct);
    }

    /// <summary>
    /// Sends an inline keyboard asking the user to confirm clearing their watchlist.
    /// </summary>
    private async Task SendClearConfirmation(long chatId, CancellationToken ct)
    {
        var confirmKeyboard = new InlineKeyboardMarkup()
            .AddNewRow()
                .AddButton("Yes", "clear_yes")
                .AddButton("No", "clear_no");

        await bot.SendMessage(chatId, "⚠️ Are you sure you want to clear your entire watchlist?", replyMarkup: confirmKeyboard, cancellationToken: ct);
    }

    /// <summary>
    /// Clears the user's watchlist and confirms.
    /// </summary>
    private async Task HandleClearConfirm(long userId, long chatId, CancellationToken ct)
    {
        movieStore.ClearMovies(userId);
        stateManager.ResetState(userId);
        await bot.SendMessage(chatId, "🗑️ Your watchlist has been cleared.", cancellationToken: ct);
    }

    /// <summary>
    /// Sends the main menu with inline keyboard buttons for Add Movie, My List, and Random Pick.
    /// </summary>
    private async Task SendMainMenu(long chatId, string text, CancellationToken ct)
    {
        var menuKeyboard = new InlineKeyboardMarkup()
            .AddNewRow()
                .AddButton("Add Movie", "add_movie")
            .AddNewRow()
                .AddButton("My List", "my_list")
            .AddNewRow()
                .AddButton("Random Pick", "random_pick");

        await bot.SendMessage(chatId, text, replyMarkup: menuKeyboard, cancellationToken: ct);
    }
}
