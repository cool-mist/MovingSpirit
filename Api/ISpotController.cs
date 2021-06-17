using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ISpotController
    {
        Task<string> Start();

        Task<string> Stop();

        Task<SpotControllerResponse> GetStatus();

    }

    public class SpotController : ISpotController
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

        public Task<SpotControllerResponse> GetStatus()
        {
            return ExecuteHttpRequest<SpotControllerResponse>("minecraft/status");
        }

        public Task<string> Start()
        {
            return ExecuteHttpRequest<string>("minecraft/start");
        }

        public Task<string> Stop()
        {
            return ExecuteHttpRequest<string>("minecraft/stop");
        }

        private async Task<T> ExecuteHttpRequest<T>(string path)
        {
            var request = await CreateHttpRequest(HttpMethod.Get, path);

            using (var response = await httpClient.SendAsync(request))
            {
                var stringContent = await response.Content.ReadAsStringAsync();

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)stringContent;
                }

                return JsonConvert.DeserializeObject<T>(stringContent);
            }
        }

        private async Task<HttpRequestMessage> CreateHttpRequest(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, path);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenProvider.GetToken());

            return request;
        }

    }

    public class SpotControllerResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("desired_capacity")]
        public int SpotCapacity { get; set; }

        public string ToString(bool capitalize = false)
        {
            if (capitalize)
            {
                return $"Instance is `{Status}`";
            }

            return $"instance is `{Status}`";
        }
    }
}
