using DSharpPlus.CommandsNext;
using MinecraftUtils.Api;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ICommandResponder
    {
        Task RespondAndArchiveAsync(Task<ITaskResponse<ICommandResponse>> command, CommandContext commandEvent);

        Task ArchiveErrorAsync(CommandsNextExtension sender, CommandErrorEventArgs commandErrorEvent);

    }
}
