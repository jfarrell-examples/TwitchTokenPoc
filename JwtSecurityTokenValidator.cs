using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TwitchTokenPoc.Services;

namespace TwitchTokenPoc
{
    public class JwtSecurityTokenValidator : ISecurityTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly KeyVaultService _keyVaultService;

        public JwtSecurityTokenValidator(IConfiguration configuration, KeyVaultService keyVaultService)
        {
            _configuration = configuration;
            _keyVaultService = keyVaultService;
        }
        
        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var signingKey = _keyVaultService.GetJwtSigningKey().Result;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(securityToken, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Issuer"],
                ValidAudience = _configuration["Audience"],
                IssuerSigningKey = securityKey
            }, out validatedToken);
        }

        public bool CanValidateToken => true;
        public int MaximumTokenSizeInBytes { get; set; }
    }
}