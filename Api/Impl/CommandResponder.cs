using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using MinecraftUtils.Api;
using MovingSpirit.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class CommandResponder : ICommandResponder
    {
        private readonly ulong serverStatusChannelId;
        private readonly ulong historyChannelId;
        private readonly TimeSpan deletionTime;

        public CommandResponder(ulong serverStatusChannelId, ulong historyChannelId, TimeSpan deletionTime)
        {
            this.serverStatusChannelId = serverStatusChannelId;
            this.historyChannelId = historyChannelId;
            this.deletionTime = deletionTime;
        }

        public async Task RespondAndArchiveAsync(Task<ITaskResponse<ICommandResponse>> command, CommandContext context)
        {
            var response = await command;

            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder
                .WithTitle("Command Results")
                .WithDescription(response?.Result?.Response)
                .WithThumbnail($"https://cdn.discordapp.com/embed/avatars/{GetThumbnail(response)}.png")
                .AddField("Spot Instance", GetState(response?.Result?.Spot?.State))
                .AddField("Minecraft Server", GetState(response?.Result?.Minecraft?.State), inline: true)
                .AddField("Players", GetPlayers(response?.Result?.Minecraft), inline: true)
                .AddField("Total Execution Time", GetTotalExecutionTime(response))
                .AddField("Breakdown", GetStats(response?.Result?.Actions));

            var discordMessage = context.Channel.SendMessageAsync(embedBuilder.Build());

            await ModifyChannelName(GetNewChannelName(response));

            await Archive(context, discordMessage);

            return;
        }

        private Task ModifyChannelName(string newChannelName)
        {
            return Task.CompletedTask;
        }

        private static string GetNewChannelName(ITaskResponse<ICommandResponse> response)
        {
            return GetState(response?.Result?.Spot?.State);
        }

        public Task ArchiveErrorAsync(CommandsNextExtension sender, CommandErrorEventArgs commandFailureEvent)
        {
            var message = "Unknown error occurred :( Retrying can help";

            if (commandFailureEvent.Exception is TaskCanceledException)
            {
                message = "Timed out. Please try again";
            }
            else if (commandFailureEvent.Exception.InnerException is RespondHelpException)
            {
                message = "Type @help for list of valid commands";
            }
            else if (commandFailureEvent.Exception is CommandNotFoundException) { }
            else if (commandFailureEvent.Exception is InvalidOperationException)
            {
                message = "Type @help for list of valid commands";
            }

            var discordMessage = commandFailureEvent.Context.RespondAsync(message);

            return Archive(commandFailureEvent.Context, discordMessage);
        }

        private Task Archive(CommandContext context, Task<DiscordMessage> message)
        {
            return DeleteMessageAfter(message, Task.FromResult(context.Message));
        }

        private async Task DeleteMessageAfter(params Task<DiscordMessage>[] messages)
        {
            await Task.Delay(this.deletionTime);

            foreach (var message in messages)
            {
                await (await message).DeleteAsync();
            }
        }

        private static string GetThumbnail(ITaskResponse<ICommandResponse> response)
        {
            // 0 = Blue
            // 4 = Red

            if (response?.Task?.Stats?.TimedOut ?? true)
            {
                return "4";
            }

            if ((response?.Task?.Stats?.Succeeded ?? false) == false)
            {
                return "4";
            }

            if ((response?.Result?.Succeeded ?? false) == false)
            {
                return "3";
            }

            return "0";
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
                bool skipAppendExecutionTime = action.Stats.ExecutionTime == TimeSpan.Zero;

                if (!skipAppendExecutionTime)
                {
                    builder.Append(" ran for");
                    builder.Append($" `{string.Format("{0:0.000}", action.Stats.ExecutionTime.TotalSeconds)} s` ");
                }

                if (action.Stats.TimedOut)
                {
                    builder.Append(" **[Timed out]**");
                }
                else if (!action.Stats.Succeeded)
                {
                    builder.Append(" **[Failed]**");
                }

                builder.Append("\n");

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
    }
}
