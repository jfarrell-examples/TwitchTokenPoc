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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://id.twitch.tv");

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
        }
    }
}