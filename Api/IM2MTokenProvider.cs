﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MovingSpirit.Api
{
    public interface IM2MTokenProvider
    {
        Task<string> GetToken();
    }

    public class M2MTokenProvider : IM2MTokenProvider
    {
        private readonly TokenRequest tokenRequest;
        private readonly HttpClient httpClient;

        public M2MTokenProvider(string clientId, string clientSecret, string audience, string tokenUrl)
        {
            this.tokenRequest = new TokenRequest()
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Audience = audience,
                GrantType = "client_credentials"
            };

            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(tokenUrl)
            };
        }

        public async Task<string> GetToken()
        {
            using (var httpResponse = await httpClient.PostAsync("", new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json")))
            {
                var stringContent = await httpResponse.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(stringContent);
                return tokenResponse.AccessToken;
            }
        }
    }

    public class TokenRequest
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

}