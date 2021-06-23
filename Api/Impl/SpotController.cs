using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class SpotController : ISpotController
    {
        private readonly IM2MTokenProvider tokenProvider;
        private readonly HttpClient httpClient;

        public SpotController(IM2MTokenProvider tokenProvider, string baseUrl)
        {
            this.tokenProvider = tokenProvider;

            this.httpClient = new HttpClient()
            {
                BaseAddress = new System.Uri(baseUrl)
            };

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        public Task<SpotControllerResponse> GetStatus(CancellationToken cancellationToken)
        {
            return ExecuteHttpRequest<SpotControllerResponse>("minecraft/status", cancellationToken);
        }

        public Task<string> Start(CancellationToken cancellationToken)
        {
            return ExecuteHttpRequest<string>("minecraft/start", cancellationToken);
        }

        public Task<string> Stop(CancellationToken cancellationToken)
        {
            return ExecuteHttpRequest<string>("minecraft/stop", cancellationToken);
        }

        private async Task<T> ExecuteHttpRequest<T>(string path, CancellationToken cancellationToken)
        {
            var request = await CreateHttpRequest(HttpMethod.Get, path, cancellationToken);
            using (var response = await httpClient.SendAsync(request, cancellationToken))
            {
                var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)stringContent;
                }

                return JsonConvert.DeserializeObject<T>(stringContent);
            }
        }

        private async Task<HttpRequestMessage> CreateHttpRequest(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, path);
            var token = (await tokenProvider.GetAccessToken(cancellationToken)).Token;

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }
    }
}
