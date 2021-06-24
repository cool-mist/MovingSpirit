using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface IMinecraftClient : IDisposable
    {
        Task<ITaskResponse<IMinecraftState>> GetStateAsync(CancellationToken cancellationToken);
    }

    public interface IMinecraftState
    {
        public string Hostname { get; }
        public string Version { get; }
        public string Motd { get; }
        public int MaxPlayers { get; }
        public int OnlinePlayers { get; }
        public bool Online { get; }
        public string Icon { get; }
    }
}