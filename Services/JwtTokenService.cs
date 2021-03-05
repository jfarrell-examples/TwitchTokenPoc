using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
            var secToken = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost",
                claims: new List<Claim>
                {
                    new Claim("accessToken", accessToken),
                    new Claim("refreshToken", refreshToken)
                },
                notBefore: null,
                expires: DateTime.Now.AddDays(1),
                signingCredentials);
            
            return new JwtSecurityTokenHandler().WriteToken(secToken);
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