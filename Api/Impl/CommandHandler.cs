using MinecraftUtils.Api;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ITaskResponse<ICommandResponse>> ExecuteAsync(BotCommand command)
        {
            var cancellationToken = new CancellationTokenSource(commandTimeout).Token;

            var response = await taskExecutor.ExecuteAsync(
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

            bool succeeded = response.Result.Actions.Select(a => a.Stats.Succeeded).Aggregate((a, b) => a && b);
            bool timedout = response.Result.Actions.Select(a => a.Stats.TimedOut).Aggregate((a, b) => a || b);

            return new TaskResponseEx(response, succeeded, timedout);
        }

        internal async Task<ICommandResponse> GetStateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            bool succeeded = true;
            var response = string.Empty;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == null)
            {
                succeeded = false;
                response = $"Failed to fetch spot state.";
            }
            else
            {
                if (spot?.State == ISpotController.RUNNING_STATE)
                {
                    minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);
                    if (minecraft?.State == null)
                    {
                        response = $"Failed to fetch minecraft server state.";
                        succeeded = false;
                    }
                }
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Status,
                Actions = allTasks.AsReadOnly(),
                Response = response,
                Succeeded = succeeded
            };
        }

        internal async Task<ICommandResponse> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            bool succeeded = true;
            var response = string.Empty;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == null)
            {
                succeeded = false;
                response = $"Failed to fetch spot state.";
            }
            else
            {
                if (spot.State == ISpotController.STOPPED_STATE)
                {
                    spot = await StartSpotInstanceStateAsync(allTasks, cancellationToken);
                    if (spot?.State == null)
                    {
                        succeeded = false;
                        response = $"Instance was {ISpotController.STOPPED_STATE}, but failed to start the instance";
                    }
                    else
                    {
                        response = $"Issued `{TaskActionNames.StartInstance}` to start the instance";
                        succeeded = true;
                    }
                }
                else if (spot?.State == ISpotController.RUNNING_STATE)
                {
                    response = $"Did not issue `{TaskActionNames.StartInstance}` because instance is already running.";
                    succeeded = false;

                    minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);

                    if (minecraft?.State == null)
                    {
                        response += $" Failed to fetch minecraft state.";
                        succeeded = false;
                    }
                }
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Start,
                Actions = allTasks.AsReadOnly(),
                Response = response,
                Succeeded = succeeded
            };
        }

        internal async Task<ICommandResponse> StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<ITaskAction> allTasks = new List<ITaskAction>();
            var response = string.Empty;
            bool succeeded = true;

            ISpotState spot = await GetSpotInstanceStateAsync(allTasks, cancellationToken);
            IMinecraftState minecraft = null;

            if (spot?.State == null)
            {
                succeeded = false;
                response = $"Failed to fetch spot state.";
            }
            else
            {
                if (spot?.State == ISpotController.RUNNING_STATE)
                {
                    minecraft = await GetMinecraftServerStateAsync(allTasks, cancellationToken);
                    if (minecraft?.State == null)
                    {
                        response = $" Failed to fetch minecraft state.";
                        succeeded = false;
                    }
                    else
                    {
                        if (minecraft?.OnlinePlayers > 0)
                        {
                            response = $"Did not issue `{TaskActionNames.StopInstance}` because `{minecraft?.OnlinePlayers}` player(s) are playing on the server right now.";
                            succeeded = false;
                        }
                        else
                        {
                            spot = await StopSpotInstanceStateAsync(allTasks, cancellationToken);
                            if (spot?.State == null)
                            {
                                succeeded = false;
                                response = $"Instance was {ISpotController.RUNNING_STATE} with no players, but failed to stop the instance";
                            }
                            else
                            {
                                response = $"Issued `{TaskActionNames.StopInstance}` to stop the instance as instance is not stopped and no players are playing on the server";
                            }
                        }
                    }
                }
            }

            return new CommandResponse()
            {
                Spot = spot,
                Minecraft = minecraft,
                Command = BotCommand.Stop,
                Actions = allTasks.AsReadOnly(),
                Response = response,
                Succeeded = succeeded
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

        internal static async Task<T> ExecuteCommandAction<T>(
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

        public bool Succeeded { get; set; }
    }

    internal class TaskResponseEx : ITaskResponse<ICommandResponse>
    {

        internal TaskResponseEx(ITaskResponse<ICommandResponse> response, bool succeeded, bool timedout)
        {
            Result = response.Result;
            Task = new CommandAction(response.Task.Name)
            {
                Stats = new TaskStatisticsEx()
                {
                    ExecutionTime = response.Task.Stats.ExecutionTime,
                    TimedOut = timedout,
                    Succeeded = succeeded,
                    Exception = response.Task.Stats.Exception
                }
            };
        }

        public ICommandResponse Result { get; }

        public ITaskAction Task { get; }
    }

    internal class TaskStatisticsEx : ITaskStatistics
    {
        public TimeSpan ExecutionTime { get; set; }

        public bool TimedOut { get; set; }

        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }
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
