using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Services
{
    public class JwtAuthService : IAuthService
    {
        private const string Issuer = "https://sts.windows.net/bbf85940-fc7b-4dae-89a8-89f27b08f9f0/";
        private const string Audience = "https://nzbwishlist.azurewebsites.net";

        private readonly Lazy<Task<TokenValidationParameters>> _paramFetcher;

        public JwtAuthService()
        {
            _paramFetcher = new Lazy<Task<TokenValidationParameters>>(() => Task.Run(LoadOpenIdConfig));
        }

        public async Task<bool> IsAuthenticated(HttpRequest request)
        {
            try
            {
                var token = GetToken(request);
                if (token == null)
                {
                    return false;
                }

                var param = await _paramFetcher.Value;
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(token, param, out _);

                return principal != null;
            }
            catch
            {
                return false;
            }
        }

        private string GetToken(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"];
            if (authHeader == StringValues.Empty || string.IsNullOrWhiteSpace(authHeader[0]))
            {
                return null;
            }

            var values = authHeader[0].Split(' ');
            if (values.Length != 2 || !values[0].Equals("Bearer", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return values[1];
        }

        private async Task<TokenValidationParameters> LoadOpenIdConfig()
        {
            var url = $"{Issuer}.well-known/openid-configuration";

            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(url, new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync();

            return new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKeys = config.SigningKeys
            };
        }
    }
}