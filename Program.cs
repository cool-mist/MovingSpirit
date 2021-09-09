using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using MovingSpirit.Api.Impl;
using MovingSpirit.Commands;
using System;
using System.Threading.Tasks;

namespace MovingSpirit
{
    class Program
    {
        public static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private async Task MainAsync(string[] args)
        {
            DiscordClient bot = CreateBot(BotContainer.CreateServiceProvider());

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }

        private DiscordClient CreateBot(IServiceProvider serviceProvider)
        {
            string token = ReadToken(serviceProvider);

            DiscordClient bot = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
            });

            var commands = bot.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "@" },
                Services = serviceProvider
            });

            commands.RegisterCommands<SpotModule>();

            commands.CommandErrored += (s, e) =>
            {
                if (e.Exception is TaskCanceledException)
                {
                    e.Context.RespondAsync("Timed out. Please try again");
                }
                else if (e.Exception.InnerException is RespondHelpException)
                {
                    e.Context.RespondAsync("Type @help for list of valid commands");
                }
                else if (e.Exception is CommandNotFoundException) { }
                else if (e.Exception is InvalidOperationException)
                {
                    e.Context.RespondAsync("Type @help for list of valid commands");
                }
                else
                {
                    e.Context.RespondAsync("Unknown error occurred. Retrying can help");
                }

                return Task.CompletedTask;
            };

            return bot;
        }

        private string ReadToken(IServiceProvider serviceProvider)
        {
            var token = Environment.GetEnvironmentVariable("MS_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Missing token");
                Environment.Exit(-1);
            }

            return token;
        }

    }
}
