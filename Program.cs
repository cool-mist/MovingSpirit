using DSharpPlus;
using DSharpPlus.CommandsNext;
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
            DiscordClient bot = CreateBot(Container.CreateServiceProvider());

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
                Console.WriteLine(e.Exception.Message);
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
