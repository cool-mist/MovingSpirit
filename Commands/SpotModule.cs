using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MovingSpirit.Api;
using System;
using System.Threading.Tasks;

namespace MovingSpirit.Commands
{
    [Group("spot")]
    public class SpotModule : BaseCommandModule
    {
        private readonly ISpotController spotController;

        public SpotModule(ISpotController spotController)
        {
            this.spotController = spotController;
        }

        [Command("status")]
        [Description("Show spot instance status")]
        public async Task StatusCommand(CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();
            await ctx.RespondAsync(statusResponse.ToString(capitalize: true));
        }

        [Command("start")]
        [Description("Start spot instance")]
        public async Task StartCommand(CommandContext ctx)
        {
            await DoTransition("Stopped", "Starting", spotController.Start, ctx);
        }

        [Command("stop")]
        [Description("Stop spot instance")]
        public async Task StopCommand(CommandContext ctx)
        {
            await DoTransition("Running", "Stopping", spotController.Stop, ctx);
        }

        private async Task DoTransition(string startState, string action, Func<Task<string>> stateFn, CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();

            if (statusResponse.Status == startState)
            {
                await ctx.RespondAsync($"{action} because {statusResponse}");
                var newStatus = await stateFn();
                await ctx.RespondAsync($"New state is `{newStatus}`");
            }
            else
            {
                await ctx.RespondAsync($"Not {action.ToLower()} because {statusResponse}");
            }
        }
    }
}
