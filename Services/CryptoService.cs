using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace TwitchTokenPoc.Services
{
    public class CryptoService
    {
        private readonly GetCredentialService _getCredentialService;
        private readonly KeyVaultService _keyVaultService;

        public CryptoService(GetCredentialService getCredentialService, KeyVaultService keyVaultService)
        {
            _getCredentialService = getCredentialService;
            _keyVaultService = keyVaultService;
        }
        
        public async Task<string> Encrypt(string rawValue)
        {
            var encryptionKey = await _keyVaultService.GetEncryptionKey();
            var cryptoClient = new CryptographyClient(encryptionKey.Id, _getCredentialService.GetKeyVaultCredentials());
            var byteData = Encoding.Unicode.GetBytes(rawValue);
            var encryptResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, byteData);
            
            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        public async Task<string> Decrypt(string encryptedValue)
        {
            var encryptionKey = await _keyVaultService.GetEncryptionKey();
            var cryptoClient = new CryptographyClient(encryptionKey.Id, _getCredentialService.GetKeyVaultCredentials());
            var encryptedBytes = Convert.FromBase64String(encryptedValue);
            var decryptResult = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encryptedBytes);
            
            return Encoding.Unicode.GetString(decryptResult.Plaintext);
        }
    }
}