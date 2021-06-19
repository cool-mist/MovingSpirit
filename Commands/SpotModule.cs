using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MovingSpirit.Api;
using System.Text;
using System.Threading.Tasks;

namespace MovingSpirit.Commands
{
    [Group("spot")]
    public class SpotModule : BaseCommandModule
    {
        private readonly ISpotController spotController;
        private readonly IMinecraftServerClient minecraftServerClient;

        public SpotModule(ISpotController spotController, IMinecraftServerClient mcServerClient)
        {
            this.spotController = spotController;
            this.minecraftServerClient = mcServerClient;
        }

        [Command("?")]
        [Description("Show spot instance status")]
        public async Task StatusCommand(CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();
            StringBuilder responseMessageBuilder = new StringBuilder();

            responseMessageBuilder.Append(statusResponse.ToString(capitalize: true));

            if (statusResponse.Status == ISpotController.RUNNING_STATE)
            {
                var serverStatus = await minecraftServerClient.GetServerStatus();

                if (serverStatus.Online)
                {
                    var activePlayers = serverStatus.OnlinePlayers;
                    responseMessageBuilder.Append($" with `{activePlayers}` players");
                }
                else
                {
                    responseMessageBuilder.Append($" but cannot fetch minecraft server status. Please try again");
                }
            }

            await ctx.RespondAsync(responseMessageBuilder.ToString());
        }

        [Command("up")]
        [Description("Start spot instance")]
        public async Task StartCommand(CommandContext ctx)
        {
            await DoTransition(ISpotController.RUNNING_STATE, ctx);
        }

        [Command("down")]
        [Description("Stop spot instance")]
        public async Task StopCommand(CommandContext ctx)
        {
            await DoTransition(ISpotController.STOPPED_STATE, ctx);
        }

        private async Task DoTransition(string targetState, CommandContext ctx)
        {
            var statusResponse = await spotController.GetStatus();

            if (targetState == ISpotController.RUNNING_STATE)
            {
                if (statusResponse.Status == ISpotController.STOPPED_STATE)
                {
                    await ctx.RespondAsync($"Starting instance");
                    var newState = await spotController.Start();
                    await ctx.RespondAsync($"New state is `{newState}`");

                    return;
                }

                await ctx.RespondAsync($"Cannot start instance because {statusResponse}");
                return;
            }

            if (targetState == ISpotController.STOPPED_STATE)
            {
                if (statusResponse.Status == ISpotController.RUNNING_STATE)
                {
                    var serverStatus = await minecraftServerClient.GetServerStatus();
                    if (serverStatus == null || !serverStatus.Online)
                    {
                        await ctx.RespondAsync($"{statusResponse.ToString(capitalize: true)}, but cannot fetch minecraft server status. Please try again");
                        return;
                    }

                    if (serverStatus.OnlinePlayers > 0)
                    {
                        await ctx.RespondAsync($"Cannot stop because {statusResponse} with `{serverStatus.OnlinePlayers}` players");
                        return;
                    }

                    await ctx.RespondAsync($"Stopping instance");
                    var newState = await spotController.Stop();
                    await ctx.RespondAsync($"New state is `{newState}`");
                    return;
                }

                await ctx.RespondAsync($"Cannot stop instance because {statusResponse}");
                return;
            }
        }
    }
}
