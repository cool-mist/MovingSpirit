using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MovingSpirit.Api;
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
            await ctx.RespondAsync($"Instance is `{statusResponse.Status}` with `{statusResponse.PlayerCount}`");
        }

        [Command("start")]
        [Description("Start spot instance")]
        public async Task StartCommand(CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();

            if (statusResponse.Status == "Stopped")
            {
                await ctx.RespondAsync($"Starting instance as current state is `{statusResponse.Status}` with `{statusResponse.PlayerCount}` player(s)");
                var newStatus = await spotController.Start();
                await ctx.RespondAsync($"New state is `{newStatus}`");
            }
            else
            {
                await ctx.RespondAsync($"Not starting the instance as current state is `{statusResponse.Status}` with `{statusResponse.PlayerCount}` player(s)");
            }
        }

        [Command("stop")]
        [Description("Stop spot instance")]
        public async Task StopCommand(CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();

            if (statusResponse.Status == "Running" && statusResponse.PlayerCount < 1)
            {
                await ctx.RespondAsync($"Stopping instance as current state is `{statusResponse}` with `{statusResponse.PlayerCount}` player(s)");
                var newStatus = await spotController.Stop();
                await ctx.RespondAsync($"New state is `{newStatus}`");
            }
            else
            {
                await ctx.RespondAsync($"Not stopping the instance as current state is `{statusResponse}` with `{statusResponse.PlayerCount}` player(s)g");
            }
        }
    }
}
