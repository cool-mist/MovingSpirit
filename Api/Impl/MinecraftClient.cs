using System;
using System.Threading;
using System.Threading.Tasks;
using static MovingSpirit.Api.Impl.SlpTcpClient;

namespace MovingSpirit.Api.Impl
{
    internal class MinecraftClient : IMinecraftClient
    {
        private readonly string serverHost;
        private readonly int serverPort = 25565;

        internal MinecraftClient(string serverHost)
        {
            this.serverHost = serverHost;
        }

        public Task<ITaskResponse<IMinecraftState>> GetStateAsync(CancellationToken cancellationToken)
        {
            return TaskExecutor.ExecuteAsync(GetServerStateAsyncTask(cancellationToken));
        }

        private Func<Task<IMinecraftState>> GetServerStateAsyncTask(CancellationToken cancellationToken)
        {
            return async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var slpTcpClient = new SlpTcpClient(serverHost, serverPort))
                {
                    var ping = await slpTcpClient.Ping(cancellationToken);
                    return CreateMinecraftServerResponse(ping);
                }
            };
        }

        private IMinecraftState CreateMinecraftServerResponse(PingPayload ping)
        {
            if (ping == null || ping.Players == null || ping.Version == null || ping.Version.Protocol == 1)
            {
                return new MinecraftState()
                {
                    Online = false,
                    MaxPlayers = 0,
                    OnlinePlayers = 0,
                    Hostname = serverHost
                };
            }

            return new MinecraftState()
            {
                Online = true,
                MaxPlayers = ping?.Players?.Max ?? 0,
                OnlinePlayers = ping?.Players?.Online ?? 0,
                Hostname = serverHost,
                Motd = ping?.Motd?.Text,
                Ping = ping
            };
        }

        public void Dispose()
        { }
    }

    internal class MinecraftState : IMinecraftState
    {
        public string Hostname { get; set; }

        public string Version { get; set; }

        public string Motd { get; set; }

        public int MaxPlayers { get; set; }

        public int OnlinePlayers { get; set; }

        public bool Online { get; set; }

        public string Icon { get; set; }

        public PingPayload Ping { get; set; }
    }
}
