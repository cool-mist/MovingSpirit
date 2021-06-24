using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class SpotController : ISpotController
    {
        private readonly ISpotTokenProvider tokenProvider;
        private readonly HttpClient httpClient;

        public SpotController(ISpotTokenProvider tokenProvider, string baseUrl)
        {
            this.tokenProvider = tokenProvider;
            this.httpClient = InitializeHttpClient(baseUrl);
        }

        public Task<ITaskResponse<ISpotState>> GetStateAsync(CancellationToken cancellationToken)
        {
            return TaskExecutor.ExecuteAsync(ExecuteHttpRequestAsyncTask("minecraft/status", cancellationToken));
        }

        public Task<ITaskResponse<ISpotState>> StartAsync(CancellationToken cancellationToken)
        {
            return TaskExecutor.ExecuteAsync(ExecuteHttpRequestAsyncTask("minecraft/start", cancellationToken));
        }

        public Task<ITaskResponse<ISpotState>> StopAsync(CancellationToken cancellationToken)
        {
            return TaskExecutor.ExecuteAsync(ExecuteHttpRequestAsyncTask("minecraft/stop", cancellationToken));
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        private static HttpClient InitializeHttpClient(string baseUrl)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new System.Uri(baseUrl)
            };

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            return httpClient;
        }

        private Func<Task<ISpotState>> ExecuteHttpRequestAsyncTask(string path, CancellationToken cancellationToken)
        {
            return () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ExecuteHttpRequestAsync(path, cancellationToken);
            };
        }

        private async Task<ISpotState> ExecuteHttpRequestAsync(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var state = string.Empty;
            var request = await CreateHttpRequest(HttpMethod.Get, path, cancellationToken);
            using (var response = await httpClient.SendAsync(request, cancellationToken))
            {
                var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (path.Contains("status"))
                {
                    state = JsonConvert.DeserializeObject<SpotStateDto>(stringContent).State;
                }
                else
                {
                    state = stringContent;
                }

                return new SpotState()
                {
                    State = state
                };
            }
        }

        private async Task<HttpRequestMessage> CreateHttpRequest(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new HttpRequestMessage(method, path);
            var token = (await tokenProvider.GetAccessToken(cancellationToken)).Token;

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        internal class SpotStateDto
        {
            [JsonProperty("status")]
            public string State { get; set; }


            [JsonProperty("desired_capacity")]
            public int SpotCapacity { get; set; }
        }

        internal class SpotState : ISpotState
        {
            public string State { get; set; }
        }
    }
}
