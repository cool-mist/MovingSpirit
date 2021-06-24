using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ICommandResponder
    {
        public Task RespondAsync(Task<ITaskResponse<ICommandResponse>> command, CommandContext ctx);
    }
}
