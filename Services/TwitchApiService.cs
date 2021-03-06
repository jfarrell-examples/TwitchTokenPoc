using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TwitchTokenPoc.Services
{
    public class TwitchApiService
    {
        private readonly IConfiguration _configuration;
        private readonly GetTokensFromHttpRequestService _getTokensFromHttpRequestService;
        private readonly TwitchAuthService _authService;

        public TwitchApiService(IConfiguration configuration, TwitchAuthService authService,
            GetTokensFromHttpRequestService getTokensFromHttpRequestService)
        {
            _configuration = configuration;
            _getTokensFromHttpRequestService = getTokensFromHttpRequestService;
            _authService = authService;
        }

        public async Task<ApiResult<TwitchUser>> GetUser(string loginName)
        {
            using var client = new HttpClient {BaseAddress = new System.Uri("https://api.twitch.tv")};
            client.DefaultRequestHeaders.Add("Client-Id", _configuration["TwitchClientId"]);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "bad-token");

            var result = new ApiResult<TwitchUser>();
            var response = await client.GetAsync($"helix/users?login={loginName}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // refresh tokens
                var (accessToken, refreshToken) = await _authService.RefreshTokens(await _getTokensFromHttpRequestService.GetRefreshToken());
                result.TokensChanged = true;
                result.NewAccessToken = accessToken;
                result.NewRefreshToken = refreshToken;
                
                // re-execute the request with the new access token
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                response = await client.GetAsync($"helix/users?login={loginName}");
            }

            if (response.IsSuccessStatusCode == false)
                throw new Exception($"GetUser request failed with status code {response.StatusCode} and reason: '{response.ReasonPhrase}'");

            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
                throw new Exception("ResponseContent was empty despite successful GetUser API call");
            
            var responseJson = JObject.Parse(responseContent);
            result.Result = responseJson["data"][0].ToObject<TwitchUser>();

            return result;
        }
    }
}