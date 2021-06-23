using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MovingSpirit.Api;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Commands
{
    [Group("spot")]
    public class SpotModule : BaseCommandModule
    {
        private readonly ISpotController spotController;
        private readonly IMinecraftServerClient minecraftServerClient;
        private readonly ICommandTimeout commandTimeout;

        public SpotModule(ISpotController spotController, IMinecraftServerClient mcServerClient, ICommandTimeout commandTimeout)
        {
            this.spotController = spotController;
            this.minecraftServerClient = mcServerClient;
            this.commandTimeout = commandTimeout;
        }

        [Command("?")]
        [Description("Show spot instance status")]
        public Task StatusCommand(CommandContext ctx)
        {
            return ExecuteWithTimeout(async (cancellationToken) =>
            {
                var statusResponse = await spotController.GetStatus(cancellationToken);
                StringBuilder responseMessageBuilder = new StringBuilder();

                responseMessageBuilder.Append(statusResponse.ToString(capitalize: true));

                if (statusResponse.Status == ISpotController.RUNNING_STATE)
                {
                    var serverStatus = await minecraftServerClient.GetServerStatus(cancellationToken);

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

                await ctx.Channel.SendMessageAsync(responseMessageBuilder.ToString());
            },
            commandTimeout);
        }

        [Command("up")]
        [Description("Start spot instance")]
        public Task StartCommand(CommandContext ctx)
        {
            return ExecuteWithTimeout(
                (cancellationToken) => DoTransition(ISpotController.RUNNING_STATE, ctx, cancellationToken),
                commandTimeout);
        }

        [Command("down")]
        [Description("Stop spot instance")]
        public Task StopCommand(CommandContext ctx)
        {
            return ExecuteWithTimeout(
                (cancellationToken) => DoTransition(ISpotController.STOPPED_STATE, ctx, cancellationToken),
                commandTimeout);
        }

        private async Task DoTransition(string targetState, CommandContext ctx, CancellationToken cancellationToken)
        {
            var statusResponse = await spotController.GetStatus(cancellationToken);

            if (targetState == ISpotController.RUNNING_STATE)
            {
                if (statusResponse.Status == ISpotController.STOPPED_STATE)
                {
                    await ctx.Channel.SendMessageAsync($"Starting instance");
                    var newState = await spotController.Start(cancellationToken);
                    await ctx.Channel.SendMessageAsync($"New state is `{newState}`");

                    return;
                }

                await ctx.Channel.SendMessageAsync($"Cannot start instance because {statusResponse}");
                return;
            }

            if (targetState == ISpotController.STOPPED_STATE)
            {
                if (statusResponse.Status == ISpotController.RUNNING_STATE)
                {
                    var serverStatus = await minecraftServerClient.GetServerStatus(cancellationToken);
                    if (serverStatus == null || !serverStatus.Online)
                    {
                        await ctx.Channel.SendMessageAsync($"{statusResponse.ToString(capitalize: true)}, but cannot fetch minecraft server status. Please try again");
                        return;
                    }

                    if (serverStatus.OnlinePlayers > 0)
                    {
                        await ctx.Channel.SendMessageAsync($"Cannot stop because {statusResponse} with `{serverStatus.OnlinePlayers}` players");
                        return;
                    }

                    await ctx.Channel.SendMessageAsync($"Stopping instance");
                    var newState = await spotController.Stop(cancellationToken);
                    await ctx.Channel.SendMessageAsync($"New state is `{newState}`");
                    return;
                }

                await ctx.Channel.SendMessageAsync($"Cannot stop instance because {statusResponse}");
                return;
            }
        }

        private static Task ExecuteWithTimeout(Func<CancellationToken, Task> fn, ICommandTimeout commandTimeout)
        {
            CancellationTokenSource src = new CancellationTokenSource(commandTimeout.TimeSpan);
            CancellationToken cancellationToken = src.Token;

            return fn(cancellationToken);
        }
    }
}
