using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBot;
using TelegramBot.Services;
using TelegramBot.StateMachine;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Bind bot token from appsettings.json "TelegramBot" section
        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.SectionName));

        // Register Telegram bot client via typed HttpClient
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;

                if (string.IsNullOrWhiteSpace(botConfig.Token))
                {
                    throw new InvalidOperationException(
                        "Telegram bot token is not configured. Set 'TelegramBot:Token' in appsettings.json.");
                }

                var options = new TelegramBotClientOptions(botConfig.Token);
                return new TelegramBotClient(options, httpClient);
            });

        // Register in-memory stores as singletons (shared across scopes)
        services.AddSingleton<MovieStore>();
        services.AddSingleton<StateManager>();

        // Register update handler as scoped (fresh instance per polling iteration)
        services.AddScoped<UpdateHandler>();

        // Register the long polling background service
        services.AddHostedService<BotService>();
    })
    .Build();

await host.RunAsync();
