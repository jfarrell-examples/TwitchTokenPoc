using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TwitchTokenPoc.Services
{
    public class TwitchAuthService
    {
        private readonly IConfiguration _configuration;

        public TwitchAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<Tuple<string, string>> GetTokensForCode(string code)
        {
            using var client = new HttpClient {BaseAddress = new Uri("https://id.twitch.tv")};

            var redirectUri = WebUtility.UrlEncode("https://localhost:5001/home/callback");
            var urlString = @$"oauth2/token?client_id={_configuration["TwitchClientId"]}"
                            + $"&client_secret={_configuration["TwitchClientSecret"]}"
                            + $"&code={code}&grant_type=authorization_code"
                            + $"&redirect_uri={redirectUri}";

            var response = await client.PostAsync(urlString, new StringContent(string.Empty));
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(responseContent);

            return new Tuple<string, string>(
                jsonObject["access_token"].ToString(),
                jsonObject["refresh_token"].ToString()
            );
        }

        public async Task<Tuple<string, string>> RefreshTokens(string refreshToken)
        {
            using var client = new HttpClient {BaseAddress = new Uri("https://id.twitch.tv")};
            var urlString = @"oauth2/token?grant_type=refresh_token"
                            + $"&refresh_token={refreshToken}"
                            + $"&client_id={_configuration["TwitchClientId"]}"
                            + $"&client_secret={_configuration["TwitchClientSecret"]}";

            var response = await client.PostAsync(urlString, new StringContent(string.Empty));
            if (response.IsSuccessStatusCode == false)
            {
                throw new Exception($"Refresh Failed: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
                throw new Exception("RefreshToken response is empty");

            
            var access_token = JObject.Parse(responseContent)["access_token"].ToString();
            var refresh_token = JObject.Parse(responseContent)["refresh_token"].ToString();

            return new Tuple<string, string>(
                access_token,
                refresh_token);
        }
    }
}