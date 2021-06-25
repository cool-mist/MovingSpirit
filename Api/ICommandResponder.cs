using DSharpPlus.CommandsNext;
using MinecraftUtils.Api;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ICommandResponder
    {
        public Task RespondAsync(Task<ITaskResponse<ICommandResponse>> command, CommandContext ctx);
    }
}
