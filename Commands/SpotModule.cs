using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MovingSpirit.Api;
using System.Threading.Tasks;

namespace MovingSpirit.Commands
{
    [Group("spot")]
    public class SpotModule : BaseCommandModule
    {
        private readonly ICommandHandler commandHandler;
        private readonly ICommandResponder commandResponder;

        public SpotModule(ICommandHandler commandHandler, ICommandResponder commandResponder)
        {
            this.commandHandler = commandHandler;
            this.commandResponder = commandResponder;
        }

        [Command("?")]
        [Description("Show spot instance status")]
        public Task StatusCommand(CommandContext ctx)
        {
            return commandResponder.RespondAsync(commandHandler.ExecuteAsync(BotCommand.Status), ctx);
        }

        [Command("up")]
        [Description("Start spot instance")]
        public Task StartCommand(CommandContext ctx)
        {
            return commandResponder.RespondAsync(commandHandler.ExecuteAsync(BotCommand.Start), ctx);
        }

        [Command("down")]
        [Description("Stop spot instance")]
        public Task StopCommand(CommandContext ctx)
        {
            return commandResponder.RespondAsync(commandHandler.ExecuteAsync(BotCommand.Stop), ctx);
        }
    }
}
