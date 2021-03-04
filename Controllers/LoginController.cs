using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TwitchTokenPoc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public IActionResult Get()
        {
            var redirectUri = WebUtility.UrlEncode("https://localhost:5001/home/callback");
            var urlString = @$"https://id.twitch.tv/oauth2/authorize?client_id={_configuration["TwitchClientId"]}"
                            + $"&redirect_uri={redirectUri}"
                            + "&response_type=code"
                            + "&scope=openid";

            return Redirect(urlString);
        }
    }
}
