using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using MovingSpirit.Api;
using MovingSpirit.Api.Impl;
using MovingSpirit.Commands;
using System;
using System.Threading.Tasks;

namespace MovingSpirit
{
    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Main method")]
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            IServiceProvider provider = BotContainer.CreateServiceProvider();
            DiscordClient bot = CreateBot(provider);

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }

        private static DiscordClient CreateBot(IServiceProvider serviceProvider)
        {
            string token = GetToken(serviceProvider);

            DiscordClient bot = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
            });

            return RegisterCommandsConfiguration(bot, serviceProvider);
        }

        private static DiscordClient RegisterCommandsConfiguration(DiscordClient bot, IServiceProvider serviceProvider)
        {
            var commands = bot.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "@" },
                Services = serviceProvider
            });

            commands.RegisterCommands<SpotModule>();

            ICommandResponder responder = serviceProvider.GetService<ICommandResponder>();
            commands.CommandErrored += responder.ArchiveErrorAsync;

            return bot;
        }

        private static string GetToken(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<IBotConfig>().BotToken;

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Missing token");
                Environment.Exit(-1);
            }

            return token;
        }
    }
}
