using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ISpotController : IDisposable
    {
        public const string RUNNING_STATE = "Running";
        public const string STOPPED_STATE = "Stopped";

        Task<ITaskResponse<ISpotState>> StartAsync(CancellationToken cancellationToken);

        Task<ITaskResponse<ISpotState>> StopAsync(CancellationToken cancellationToken);

        Task<ITaskResponse<ISpotState>> GetStateAsync(CancellationToken cancellationToken);
    }

    public interface ISpotState
    {
        public string State { get; }
    }

    public interface IAccessToken
    {
        public string Token { get; }
    }

    public interface ISpotTokenProvider
    {
        Task<IAccessToken> GetAccessToken(CancellationToken cancellationToken);
    }
}
