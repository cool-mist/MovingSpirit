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

        public CommandHandler(ISpotController spotController, IMinecraftClient minecraftClient, TimeSpan commandTimeout)
        {
            this.spotController = spotController;
            this.minecraftClient = minecraftClient;
            this.commandTimeout = commandTimeout;
        }

        public Task<ICommandResponse> ExecuteAsync(BotCommand command)
        {
            var cancellationToken = new CancellationTokenSource(commandTimeout).Token;

            switch (command)
            {
                case BotCommand.Status: return GetStateAsync(cancellationToken);
                case BotCommand.Start: return StartAsync(cancellationToken);
                case BotCommand.Stop: return StopAsync(cancellationToken);
                default: return GetStateAsync(cancellationToken);
            }
        }

        internal async Task<ICommandResponse> GetStateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ICommandAction> actions = new List<ICommandAction>();

            ISpotState spot = await GetSpotInstanceStateAsync(actions, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(actions, cancellationToken);
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Status,
                Actions = actions.AsReadOnly()
            };
        }

        internal async Task<ICommandResponse> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ICommandAction> actions = new List<ICommandAction>();

            ISpotState spot = await GetSpotInstanceStateAsync(actions, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.STOPPED_STATE)
            {
                spot = await StartSpotInstanceStateAsync(actions, cancellationToken);
            }
            else if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(actions, cancellationToken);
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Start,
                Actions = actions.AsReadOnly()
            };
        }

        internal async Task<ICommandResponse> StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ICommandAction> actions = new List<ICommandAction>();

            ISpotState spot = await GetSpotInstanceStateAsync(actions, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == ISpotController.RUNNING_STATE)
            {
                minecraft = await GetMinecraftServerStateAsync(actions, cancellationToken);
            }

            if (minecraft == null || minecraft.OnlinePlayers <= 0)
            {
                spot = await StopSpotInstanceStateAsync(actions, cancellationToken);
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Stop,
                Actions = actions.AsReadOnly()
            };
        }

        private Task<ISpotState> GetSpotInstanceStateAsync(List<ICommandAction> actions, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                CommandActions.GetInstanceState,
                () => spotController.GetStateAsync(cancellationToken),
                actions);
        }

        private Task<ISpotState> StartSpotInstanceStateAsync(List<ICommandAction> actions, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                CommandActions.StartInstance,
                () => spotController.StartAsync(cancellationToken),
                actions);
        }

        private Task<ISpotState> StopSpotInstanceStateAsync(List<ICommandAction> actions, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                CommandActions.StopInstance,
                () => spotController.StopAsync(cancellationToken),
                actions);
        }

        private Task<IMinecraftState> GetMinecraftServerStateAsync(List<ICommandAction> actions, CancellationToken cancellationToken)
        {
            return ExecuteCommandAction(
                CommandActions.GetServerState,
                () => minecraftClient.GetStateAsync(cancellationToken),
                actions);
        }

        internal async Task<T> ExecuteCommandAction<T>(
            CommandActions actionName,
            Func<Task<ITaskResponse<T>>> task,
            List<ICommandAction> actions) where T : class
        {
            CommandAction action = new CommandAction(actionName.ToString());
            actions.Add(action);

            var response = await task.Invoke();
            action.Stats = response?.Stats;

            return response.Result;
        }
    }

    internal class CommandResponse : ICommandResponse
    {
        public ISpotState Spot { get; set; }

        public IMinecraftState Minecraft { get; set; }

        public BotCommand Command { get; set; }

        public IReadOnlyCollection<ICommandAction> Actions { set; get; }
    }

    internal class CommandAction : ICommandAction
    {
        public CommandAction(string actionName)
        {
            Name = actionName;
        }

        public string Name { get; }

        public ITaskStatistics Stats { get; set; }
    }

    internal enum CommandActions
    {
        None = 0,
        GetInstanceState,
        StartInstance,
        StopInstance,
        GetServerState
    }
}
