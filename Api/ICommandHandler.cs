using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ICommandHandler
    {
        Task<ICommandResponse> ExecuteAsync(BotCommand command);
    }

    public enum BotCommand
    {
        None = 0,
        Start,
        Stop,
        Status
    }

    public interface ICommandResponse
    {
        ISpotState Spot { get; }

        IMinecraftState Minecraft { get; }

        BotCommand Command { get; }

        IReadOnlyCollection<ICommandAction> Actions { get; }
    }

    public interface ICommandAction
    {
        public string Name { get; }

        public ITaskStatistics Stats { get; }
    }
}
