using System;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace TwitchTokenPoc.Services
{
    public class KeyVaultService
    {
        private readonly GetCredentialService _getCredentialService;
        private readonly IConfiguration _configuration;

        public KeyVaultService(GetCredentialService getCredentialService, IConfiguration configuration)
        {
            _getCredentialService = getCredentialService;
            _configuration = configuration;
        }

        private SecretClient SecretClient => new SecretClient(
            vaultUri: new Uri(_configuration["KeyVaultUri"]),
            credential: _getCredentialService.GetKeyVaultCredentials());
        
        private KeyClient KeyClient => new KeyClient(
            vaultUri: new Uri(_configuration["KeyVaultUri"]),
            credential: _getCredentialService.GetKeyVaultCredentials());

        public async Task<string> GetJwtSigningKey()
        {
            var secretResult = await SecretClient.GetSecretAsync("jwt-key");
            return secretResult.Value.Value;
        }

        public async Task<KeyVaultKey> GetEncryptionKey()
        {
            var keyResponse = await KeyClient.GetKeyAsync("encryption-key");
            return keyResponse.Value;
        }
    }
}