using MinecraftUtils.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ICommandHandler
    {
        Task<ITaskResponse<ICommandResponse>> ExecuteAsync(BotCommand command);
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

        string Response { get; }

        bool Succeeded { get; }

        IReadOnlyCollection<ITaskAction> Actions { get; }
    }
}
