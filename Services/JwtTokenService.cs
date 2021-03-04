using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TwitchTokenPoc.Services
{
    public class JwtTokenService
    {
        private readonly KeyVaultService _keyVaultService;
        private readonly CryptoService _cryptoService;

        public JwtTokenService(KeyVaultService keyVaultService, CryptoService cryptoService)
        {
            _keyVaultService = keyVaultService;
            _cryptoService = cryptoService;
        }
        
        public async Task<string> CreateJwtTokenString(string accessToken, string refreshToken)
        {
            var jwtSigningKey = await _keyVaultService.GetJwtSigningKey();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var header = new JwtHeader(signingCredentials);
            var payload = new JwtPayload
            {
                { "accessToken", accessToken },
                { "refreshToken", refreshToken }
            };
            
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // Token to String so you can use it in your client
            return handler.WriteToken(secToken);
        }

        public async Task<Tuple<string, string>> GetTokensFromTokenString(string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);
            var accessToken = token.Claims.FirstOrDefault(x => x.Type == "accessToken");
            var refreshToken = token.Claims.FirstOrDefault(x => x.Type == "refreshToken");

            return new Tuple<string, string>(
                await _cryptoService.Decrypt(accessToken.Value),
                await _cryptoService.Decrypt(refreshToken.Value));
        }
    }
}