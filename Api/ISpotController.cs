using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface ISpotController
    {
        Task<string> Start();

        Task<string> Stop();

        Task<SpotControllerResponse> GetStatus();

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
