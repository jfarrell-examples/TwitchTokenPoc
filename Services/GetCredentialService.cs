using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace TwitchTokenPoc.Services
{
    public class GetCredentialService
    {
        private readonly IConfiguration _configuration;
        private TokenCredential _tokenCredential;

        public GetCredentialService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public TokenCredential GetKeyVaultCredentials()
        {
            return _tokenCredential ??= new ClientSecretCredential(
                tenantId: _configuration["TenantId"],
                clientId: _configuration["ClientId"],
                clientSecret: _configuration["ClientSecret"]);
        }
    }
}