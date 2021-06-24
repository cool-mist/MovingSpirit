using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class CommandResponder : ICommandResponder
    {
        public async Task RespondAsync(Task<ICommandResponse> command, CommandContext ctx)
        {
            var response = await command;

            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.WithTitle("Command Results")
                .AddField("Spot Instance", GetState(response?.Spot?.State))
                .AddField("Minecraft Server", GetState(response?.Minecraft?.Online), inline: true)
                .AddField("Players", GetPlayers(response?.Minecraft), inline: true)
                .AddField("Stats", GetStats(response?.Actions));

            await ctx.Channel.SendMessageAsync(embedBuilder.Build());
        }

        private string GetStats(IReadOnlyCollection<ICommandAction> actions)
        {
            if (actions?.Count == 0)
            {
                return "No tasks run";
            }

            int idx = 1;
            StringBuilder builder = new StringBuilder();
            foreach (ICommandAction action in actions)
            {
                builder.Append($"**{idx}**. `{action.Name}`");

                if (action.Stats.TimedOut)
                {
                    builder.Append(" timed out after");
                }
                else if (action.Stats.Succeeded)
                {
                    builder.Append(" ran in");
                }
                else
                {
                    builder.Append(" failed in");
                }

                builder.Append($" `{string.Format("{0:0.000}", action.Stats.ExecutionTime.TotalSeconds)}` s\n");

                idx++;
            }

            return builder.ToString();
        }

        private string GetPlayers(IMinecraftState minecraft)
        {
            if (minecraft == null || !minecraft.Online)
            {
                return "NA";
            }

            return $"{minecraft.OnlinePlayers}/{minecraft.MaxPlayers}";
        }

        internal static string GetState(string state)
        {
            if (state == null)
            {
                return "Did not fetch";
            }

            return state;
        }

        internal static string GetState(bool? state)
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
