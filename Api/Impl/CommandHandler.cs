using MinecraftUtils.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class CommandHandler : ICommandHandler
    {
        private readonly ISpotController spotController;
        private readonly IMinecraftClient minecraftClient;
        private readonly TimeSpan commandTimeout;
        private IBotConfig botConfig;
        private ITaskExecutor taskExecutor;

        public CommandHandler(
            ISpotController spotController,
            IMinecraftClient minecraftClient,
            TimeSpan commandTimeout,
            IBotConfig botConfig,
            ITaskExecutor taskExecutor)
        {
            this.spotController = spotController;
            this.minecraftClient = minecraftClient;
            this.commandTimeout = commandTimeout;
            this.botConfig = botConfig;
            this.taskExecutor = taskExecutor;
        }

        public Task<ITaskResponse<ICommandResponse>> ExecuteAsync(BotCommand command)
        {
            var cancellationToken = new CancellationTokenSource(commandTimeout).Token;

            return taskExecutor.ExecuteAsync(
                command.ToString(),
                () =>
                {
                    return command switch
                    {
                        BotCommand.Status => GetStateAsync(cancellationToken),
                        BotCommand.Start => StartAsync(cancellationToken),
                        BotCommand.Stop => StopAsync(cancellationToken),
                        BotCommand.None => GetStateAsync(cancellationToken),
                        _ => GetStateAsync(cancellationToken),
                    };
                },
                cancellationToken);
        }

        internal async Task<ICommandResponse> GetStateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            var response = string.Empty;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Status,
                Actions = allTasks.AsReadOnly(),
                Response = response
            };
        }

        internal async Task<ICommandResponse> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            var response = string.Empty;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.STOPPED_STATE)
            {
                spot = await StartSpotInstanceStateAsync(allTasks, cancellationToken);
                response = $"Issued `{TaskActionNames.StartInstance}` to start the instance";
            }
            else if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);
                response = $"Did not issue `{TaskActionNames.StartInstance}` because instance is already running";
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Start,
                Actions = allTasks.AsReadOnly(),
                Response = response
            };
        }

        internal async Task<ICommandResponse> StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            var response = string.Empty;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);

            }

            if (spot?.State != ISpotController.STOPPED_STATE && minecraft?.OnlinePlayers <= 0)
            {
                spot = await StopSpotInstanceStateAsync(allTasks, cancellationToken);
                response = $"Issued `{TaskActionNames.StopInstance}` to stop the instance as instance is not stopped and no players are playing on the server";
            }
            else
            {
                response = $"Did not issue `{TaskActionNames.StopInstance}` because {minecraft?.OnlinePlayers} player(s) are still playing on the server";
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Stop,
                Actions = allTasks.AsReadOnly(),
                Response = response
            };
        }

        private Task<ISpotState> GetSpotInstanceStateAsync(List<ITaskAction> allTasks, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                () => spotController.GetStateAsync(cancellationToken),
                allTasks);
        }

        private Task<ISpotState> StartSpotInstanceStateAsync(List<ITaskAction> allTasks, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                () => spotController.StartAsync(cancellationToken),
                allTasks);
        }

        private Task<ISpotState> StopSpotInstanceStateAsync(List<ITaskAction> allTasks, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                () => spotController.StopAsync(cancellationToken),
                allTasks);
        }

        private Task<IMinecraftState> GetMinecraftServerStateAsync(List<ITaskAction> actions, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                () => minecraftClient.GetStateAsync(botConfig.MinecraftServerName, cancellationToken),
                actions);
        }

        internal async Task<T> ExecuteCommandAction<T>(
            Func<Task<ITaskResponse<T>>> task,
            List<ITaskAction> allTasks) where T : class
        {
            var response = await task.Invoke();
            allTasks.Add(response.Task);

            return response.Result;
        }
    }

    internal class CommandResponse : ICommandResponse
    {
        public ISpotState Spot { get; set; }

        public IMinecraftState Minecraft { get; set; }

        public BotCommand Command { get; set; }

        public IReadOnlyCollection<ITaskAction> Actions { set; get; }

        public string Response { get; set; }
    }

    internal class CommandAction : ITaskAction
    {
        public CommandAction(string actionName)
        {
            Name = actionName;
        }

        public string Name { get; }

        public ITaskStatistics Stats { get; set; }
    }


}
