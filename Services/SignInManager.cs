using JwtAuth.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JwtAuth.Services
{
    public class SignInManager
    {
        private readonly ILogger<SignInManager> _logger;
        private readonly ApplicationDbContext _ctx;
        private readonly JwtAuthService _JwtAuthService;
        private readonly JwtTokenConfig _jwtTokenConfig;

        public SignInManager(ILogger<SignInManager> logger,
                             JwtAuthService JWTAuthService,
                             JwtTokenConfig jwtTokenConfig,
                             ApplicationDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
            _JwtAuthService = JWTAuthService;
            _jwtTokenConfig = jwtTokenConfig;
        }

        //We sign in the user using the SignIn Method.
        public async System.Threading.Tasks.Task<SignInResult> SignIn(string userName, string password)
        {
            _logger.LogInformation($"Validating user [{userName}]", userName);

            SignInResult result = new SignInResult();

            if (string.IsNullOrWhiteSpace(userName)) return result;
            if (string.IsNullOrWhiteSpace(password)) return result;

            //We query the database to check if the user with the given email & password exists.
            var user = await _ctx.Users.Where(f => f.email == userName && f.password == password).FirstOrDefaultAsync();
            if (user != null)
            {
                //If the user exists. we build the claim.
                var claims = BuildClaims(user);
                result.User = user;

                //We use JwtAuthService to build the Access token & Refresh token.
                result.AccessToken = _JwtAuthService.BuildToken(claims);
                result.RefreshToken = _JwtAuthService.BuildRefreshToken();

                //Save the RefreshTokens to database
                _ctx.RefreshTokens.Add(new RefreshToken { UserId = user.Id, Token = result.RefreshToken, issuedAt = DateTime.Now, expiresAt = DateTime.Now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration) });
                _ctx.SaveChanges();

                result.Success = true;
            };

            return result;
        }

        //RefreshToken method validates current Access Token & Refresh Token and generates the new Access token & Refresh token,
        public async System.Threading.Tasks.Task<SignInResult> RefreshToken(string AccessToken, string RefreshToken)
        {

            //First, we use the current Access Token to re-create the claimsPrincipal.
            ClaimsPrincipal claimsPrincipal = _JwtAuthService.GetPrincipalFromToken(AccessToken);
            SignInResult result = new SignInResult();

            if (claimsPrincipal == null) return result;

            //We access the id of the user from the claimsPrincipal and query to check if the user exists.
            string id = claimsPrincipal.Claims.First(c => c.Type == "id").Value;
            var user = await _ctx.Users.FindAsync(Convert.ToInt32(id));

            if (user == null) return result;

            //Next, we query the RefreshTokens table to check if Refresh token belongs to the user and not yet expired.
            var token = await _ctx.RefreshTokens
                    .Where(f => f.UserId == user.Id
                            && f.Token == RefreshToken
                            && f.expiresAt >= DateTime.Now)
                    .FirstOrDefaultAsync();

            if (token == null) return result;

            //If all is ok, we build the Claims and create a new Access token, and Refresh token. The old Refresh token is deleted and the new token is saved to the database in its place.
            var claims = BuildClaims(user);

            result.User = user;
            result.AccessToken = _JwtAuthService.BuildToken(claims);
            result.RefreshToken = _JwtAuthService.BuildRefreshToken();

            _ctx.RefreshTokens.Remove(token);
            _ctx.RefreshTokens.Add(new RefreshToken { UserId = user.Id, Token = result.RefreshToken, issuedAt = DateTime.Now, expiresAt = DateTime.Now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration) });
            _ctx.SaveChanges();

            result.Success = true;

            return result;
        }

        //BuildClaims method builds the claims of the user. Here you can add additional claims to it.
        private Claim[] BuildClaims(User user)
        {
            //User is Valid
            var claims = new[]
            {
                new Claim("id",user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.email)
 
                //Add Custom Claims here
            };

            return claims;
        }

    }

    public class SignInResult
    {
        public bool Success { get; set; }
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public SignInResult()
        {
            Success = false;
        }
    }
}
