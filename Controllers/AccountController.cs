using JwtAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {

        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager _signInManager;

        public AccountController(ILogger<AccountController> logger,
                                SignInManager signInManager,
                                JwtAuthService jwtAuthManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {

            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var result = await _signInManager.SignIn(request.UserName, request.Password);

            if (!result.Success) return Unauthorized();

            _logger.LogInformation($"User [{request.UserName}] logged in the system.");

            return Ok(new LoginResult
            {
                UserName = result.User.email,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        [HttpPost("refreshtoken")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var result = await _signInManager.RefreshToken(request.AccessToken, request.RefreshToken);

            if (!result.Success) return Unauthorized();

            return Ok(new LoginResult
            {
                UserName = result.User.email,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

}

