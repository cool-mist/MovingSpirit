using System.Threading;
using System.Threading.Tasks;
using static MovingSpirit.Api.Impl.SlpTcpClient;

namespace MovingSpirit.Api.Impl
{
    internal class MinecraftServerClient : IMinecraftServerClient
    {
        private string serverHost;
        private int serverPort = 25565;

        internal MinecraftServerClient(string serverHost)
        {
            this.serverHost = serverHost;
        }

        public async Task<IMinecraftServer> GetServerStatus(CancellationToken cancellationToken)
        {
            using (var slpTcpClient = new SlpTcpClient(serverHost, serverPort))
            {
                var ping = await slpTcpClient.Ping(cancellationToken);
                return CreateMinecraftServerResponse(ping);
            }
        }

        private IMinecraftServer CreateMinecraftServerResponse(PingPayload ping)
        {
            if (ping == null || ping.Players == null || ping.Version == null || ping.Version.Protocol == 1)
            {
                return new MinecraftServer()
                {
                    Online = false,
                    MaxPlayers = 0,
                    OnlinePlayers = 0,
                    Hostname = serverHost
                };
            }

            return new MinecraftServer()
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
}
