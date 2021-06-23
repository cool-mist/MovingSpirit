using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface IMinecraftServerClient : IDisposable
    {
        Task<IMinecraftServer> GetServerStatus(CancellationToken cancellationToken);
    }
}