﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class M2MTokenProvider : ISpotTokenProvider
    {
        private readonly TokenRequest tokenRequest;
        private readonly HttpClient httpClient;

        public M2MTokenProvider(IBotConfig botConfig)
        {
            this.tokenRequest = new TokenRequest()
            {
                ClientId = botConfig.ClientId,
                ClientSecret = botConfig.ClientSecret,
                Audience = botConfig.TokenAudience,
                GrantType = "client_credentials"
            };

            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(botConfig.TokenBaseUrl)
            };
        }

        public async Task<IAccessToken> GetAccessToken(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var httpResponse = await httpClient.PostAsync(
                "/oauth/token",
                new StringContent(
                    JsonConvert.SerializeObject(tokenRequest),
                    Encoding.UTF8,
                    "application/json"),
                cancellationToken))
            {
                var stringContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(stringContent);
                return new AccessToken(tokenResponse.AccessToken);
            }
        }
    }

    internal class TokenRequest
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

    internal class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    internal class AccessToken : IAccessToken
    {
        private readonly string accessToken;

        internal AccessToken(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public string Token => accessToken;
    }
}
