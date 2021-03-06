using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TwitchTokenPoc.Services
{
    public class GetTokensFromHttpRequestService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CryptoService _cryptoService;

        public GetTokensFromHttpRequestService(IHttpContextAccessor httpContextAccessor, CryptoService cryptoService)
        {
            _httpContextAccessor = httpContextAccessor;
            _cryptoService = cryptoService;
        }

        public async Task<string> GetAccessToken()
        {
            var encryptedAccessToken = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "accessToken")?.Value;
            if (string.IsNullOrEmpty(encryptedAccessToken))
                throw new InvalidOperationException("Request does not contain claim for access token");

            var accessToken = await _cryptoService.Decrypt(encryptedAccessToken);
            return accessToken;
        }

        public async Task<string> GetRefreshToken()
        {
            var encryptedRefreshToken = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value;
            if (string.IsNullOrEmpty(encryptedRefreshToken))
                throw new InvalidOperationException("Request does not contain claim for refresh token");

            var refreshToken = await _cryptoService.Decrypt(encryptedRefreshToken);
            return refreshToken;
        }
    }
}