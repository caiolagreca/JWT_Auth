using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JwtAuth.Services
{
    public class JwtAuthService
    {
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly ILogger<JwtAuthService> _logger;

        public JwtAuthService(JwtTokenConfig jwtTokenConfig,
                              ILogger<JwtAuthService> logger)
        {
            _jwtTokenConfig = jwtTokenConfig;
            _logger = logger;
        }

        //BuildToken method creates the access token. We need to pass the list of claims to it.
        public string BuildToken(Claim[] claims)
        {
            //We create the SigningCredentials from the Secret, which we get from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //Now we create the token with the values from appsettings.json and signingCredentials
            var token = new JwtSecurityToken(
                    issuer: _jwtTokenConfig.Issuer,
                    audience: _jwtTokenConfig.Audience,
                    notBefore: DateTime.Now,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),
                    signingCredentials: creds);

            //Finally we serialize the security token into a string and return it back.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //The method BuildRefreshToken builds the Refresh Token
        public string BuildRefreshToken()
        {
            //The criterias to create a refresh token is that it must be reasonably unique & not easy to guess for anyone. Hence, we use a 32 byte long random number and convert it to base64 so that we can use it as a string.
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        //the method below recreates the User ( ClaimsPrincipal ) from the token.
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            JwtSecurityTokenHandler tokenValidator = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false
            };

            try
            {
                //We use the ValidateToken method below to verify the access token while issuing the refresh token.
                var principal = tokenValidator.ValidateToken(token, parameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogError($"Token validation failed");
                    return null;
                }

                return principal;
            }
            catch (Exception e)
            {
                _logger.LogError($"Token validation failed: {e.Message}");
                return null;
            }
        }
    }

    public class JwtTokenConfig //We inject the jwtTokenConfig, which reads the configuration from the appsettings.json
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }

    }
}
