using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
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
        
        public async Task<IActionResult> Get([FromHeader(Name = "X-Token")]string tokenString)
        {
            var (decryptedAccessToken, decryptedRefreshToken) = await _jwtTokenService.GetTokensFromTokenString(tokenString);

            return Ok(new
            {
                decryptedAccessToken,
                decryptedRefreshToken
            });
        }
    }
}