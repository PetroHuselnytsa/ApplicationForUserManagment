using Microsoft.AspNetCore.Mvc;
using TestFirstProject.DTOs.Auth;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Controllers
{
    /// <summary>
    /// Handles user registration and authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.RegisterAsync(request);
            return Created(string.Empty, response);
        }

        /// <summary>
        /// Authenticate a user and return a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
    }
}
