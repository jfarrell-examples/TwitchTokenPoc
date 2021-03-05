using System;
using System.Linq;
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
        private readonly CryptoService _cryptoService;
        private readonly JwtTokenService _jwtTokenService;

        public HomeController(TwitchAuthService authService, JwtTokenService jwtTokenService, CryptoService cryptoService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
            _cryptoService = cryptoService;
        }
        
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var (accessToken, refreshToken) = await _authService.GetTokensForCode(code);
            var encodedAccessToken = await _cryptoService.Encrypt(accessToken);
            var encodedRefreshToken = await _cryptoService.Encrypt(refreshToken);

            var jwtTokenString = await _jwtTokenService.CreateJwtTokenString(encodedAccessToken, encodedRefreshToken);
            return Ok(jwtTokenString);
        }
        
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var encryptedAccessToken = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessToken");
            var encryptedRefreshToken = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "refreshToken");

            return Ok(new
            {
                a_t = await _cryptoService.Decrypt(encryptedAccessToken.Value),
                r_t = await _cryptoService.Decrypt(encryptedRefreshToken.Value)
            });
        }
    }
}