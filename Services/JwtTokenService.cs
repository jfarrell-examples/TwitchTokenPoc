using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TwitchTokenPoc.Services
{
    public class JwtTokenService
    {
        private readonly KeyVaultService _keyVaultService;
        private readonly CryptoService _cryptoService;
        private readonly IConfiguration _configuration;

        public JwtTokenService(KeyVaultService keyVaultService, CryptoService cryptoService, IConfiguration configuration)
        {
            _keyVaultService = keyVaultService;
            _cryptoService = cryptoService;
            _configuration = configuration;
        }
        
        public async Task<string> CreateJwtTokenString(string accessToken, string refreshToken)
        {
            var jwtSigningKey = await _keyVaultService.GetJwtSigningKey();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var secToken = new JwtSecurityToken(
                issuer: _configuration["Issuer"],
                audience: _configuration["Audience"],
                claims: new List<Claim>
                {
                    new Claim("accessToken", await _cryptoService.Encrypt(accessToken)),
                    new Claim("refreshToken", await _cryptoService.Encrypt(refreshToken))
                },
                notBefore: null,
                expires: DateTime.Now.AddDays(1),
                signingCredentials);
            
            return new JwtSecurityTokenHandler().WriteToken(secToken);
        }
    }
}