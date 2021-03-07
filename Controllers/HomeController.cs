using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchTokenPoc.Services;

namespace TwitchTokenPoc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly TwitchAuthService _authService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly TwitchApiService _twitchClient;

        public HomeController(TwitchAuthService authService, JwtTokenService jwtTokenService, TwitchApiService twitchApiService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
            _twitchClient = twitchApiService;
        }
        
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var (accessToken, refreshToken) = await _authService.GetTokensForCode(code);
            var jwtTokenString = await _jwtTokenService.CreateJwtTokenString(accessToken, refreshToken);
            return Ok(jwtTokenString);
        }
        
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var getUserResult = await _twitchClient.GetUser("jfarrell1983");

            return Ok(getUserResult);
        }
    }
}