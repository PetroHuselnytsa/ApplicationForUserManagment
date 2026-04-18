using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestFirstProject.DTOs;
using TestFirstProject.Services;

namespace TestFirstProject.Controllers
{
    /// <summary>
    /// Authentication controller handling registration, login, token refresh, logout,
    /// and email verification. All endpoints return structured error responses.
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// Returns access and refresh tokens on success.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new ErrorResponse(400, "Validation failed.", errors));
            }

            var (token, error) = await _authService.RegisterAsync(request);

            if (error != null)
            {
                return StatusCode(error.StatusCode, error);
            }

            return StatusCode(StatusCodes.Status201Created, token);
        }

        /// <summary>
        /// Authenticates a user and returns access and refresh tokens.
        /// Returns 429 if the account is locked due to failed attempts.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new ErrorResponse(400, "Validation failed.", errors));
            }

            var (token, error) = await _authService.LoginAsync(request);

            if (error != null)
            {
                return StatusCode(error.StatusCode, error);
            }

            return Ok(token);
        }

        /// <summary>
        /// Refreshes an access token using a valid refresh token.
        /// Implements token rotation: the old refresh token is revoked and a new pair is issued.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new ErrorResponse(400, "Validation failed.", errors));
            }

            var (token, error) = await _authService.RefreshTokenAsync(request);

            if (error != null)
            {
                return StatusCode(error.StatusCode, error);
            }

            return Ok(token);
        }

        /// <summary>
        /// Revokes a refresh token, effectively logging the user out.
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new ErrorResponse(400, "Validation failed.", errors));
            }

            var error = await _authService.LogoutAsync(request);

            if (error != null)
            {
                return StatusCode(error.StatusCode, error);
            }

            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Verifies a user's email address using the verification token
        /// sent during registration.
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new ErrorResponse(400, "Validation failed.", errors));
            }

            var error = await _authService.VerifyEmailAsync(request);

            if (error != null)
            {
                return StatusCode(error.StatusCode, error);
            }

            return Ok(new { message = "Email verified successfully." });
        }
    }
}
