using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ISpotController
    {
        public const string RUNNING_STATE = "Running";
        public const string STOPPED_STATE = "Stopped";

        Task<string> Start(CancellationToken cancellationToken);

        Task<string> Stop(CancellationToken cancellationToken);

        Task<SpotControllerResponse> GetStatus(CancellationToken cancellationToken);

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

        public override string ToString()
        {
            return ToString(capitalize: false);
        }
    }
}
