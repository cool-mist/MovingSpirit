using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MinecraftUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class CommandResponder : ICommandResponder
    {
        public async Task RespondAsync(Task<ITaskResponse<ICommandResponse>> command, CommandContext ctx)
        {
            var response = await command;

            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder
                .WithTitle("Command Results")
                .WithDescription(response?.Result?.Response)
                .AddField("Spot Instance", GetState(response?.Result?.Spot?.State))
                .AddField("Minecraft Server", GetState(response?.Result?.Minecraft?.State), inline: true)
                .AddField("Players", GetPlayers(response?.Result?.Minecraft), inline: true)
                .AddField("Total Execution Time", GetTotalExecutionTime(response))
                .AddField("Breakdown", GetStats(response?.Result?.Actions));

            await ctx.Channel.SendMessageAsync(embedBuilder.Build());
        }

        private static string GetTotalExecutionTime(ITaskResponse<ICommandResponse> response)
        {
            return $"`{string.Format("{0:0.000}", response.Task.Stats.ExecutionTime.TotalSeconds)} s`";
        }

        private static string GetStats(IReadOnlyCollection<ITaskAction> actions)
        {
            if (actions?.Count == 0)
            {
                return "No tasks run";
            }

            int idx = 1;
            StringBuilder builder = new StringBuilder();
            foreach (ITaskAction action in actions)
            {
                builder.Append($"**{idx}**. `{action.Name}`");
                bool appendExecutionTime = true;
                if (action.Stats.TimedOut)
                {
                    builder.Append(" timed out after");
                }
                else if (action.Stats.Succeeded)
                {
                    builder.Append(" ran for");
                }
                else if (action.Stats.ExecutionTime == TimeSpan.Zero)
                {
                    builder.Append(" did not run");
                    appendExecutionTime = false;
                }

                if (appendExecutionTime)
                {
                    builder.Append($" `{string.Format("{0:0.000}", action.Stats.ExecutionTime.TotalSeconds)} s`\n");
                }

                idx++;
            }

            return builder.ToString();
        }

        private static string GetPlayers(IMinecraftState minecraft)
        {
            if (minecraft?.State == null)
            {
                return "NA";
            }

            return $"{minecraft.OnlinePlayers}/{minecraft.MaxPlayers}";
        }

        private static string GetState(string state)
        {
            if (state == null)
            {
                return "Did not fetch";
            }

            return state;
        }

        private static string GetState(bool? state)
        {
            if (state == null)
            {
                return "Did not fetch";
            }

            if (state ?? false)
            {
                return "Online";
            }

            return "Offline";
        }
    }
}
