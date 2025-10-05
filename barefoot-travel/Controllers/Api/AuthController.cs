using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs.Auth;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Access token and refresh token</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

                var tokens = await _authService.LoginAsync(loginDto.Username, loginDto.Password);

                _logger.LogInformation("Login successful for username: {Username}", loginDto.Username);

                return Ok(new ApiResponse(true, "Login successful", tokens));
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request during login: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Unauthorized login attempt for username: {Username}", loginDto.Username);
                return Unauthorized(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
                return StatusCode(500, new ApiResponse(false, "An error occurred during login"));
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token</param>
        /// <returns>New access token and refresh token</returns>
        /// <response code="200">Token refresh successful</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Invalid or expired refresh token</response>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                var tokens = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

                _logger.LogInformation("Token refresh successful");

                return Ok(new ApiResponse(true, "Token refresh successful", tokens));
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request during token refresh: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Unauthorized token refresh attempt");
                return Unauthorized(new ApiResponse(false, ex.Message));
            }
            catch (NotImplementedException ex)
            {
                _logger.LogWarning("Refresh token not implemented: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, "Refresh token functionality is not implemented"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponse(false, "An error occurred during token refresh"));
            }
        }

        /// <summary>
        /// Logout and invalidate refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token to invalidate</param>
        /// <returns>Success message</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("logout")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                _logger.LogInformation("Logout attempt for user: {UserId}", userId);

                await _authService.LogoutAsync(refreshTokenDto.RefreshToken);

                _logger.LogInformation("Logout successful for user: {UserId}", userId);

                return Ok(new ApiResponse(true, "Logout successful"));
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request during logout: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse(false, "An error occurred during logout"));
            }
        }

        /// <summary>
        /// Register new user account
        /// </summary>
        /// <param name="registerDto">Registration data</param>
        /// <returns>Created user profile</returns>
        /// <response code="200">Account created successfully</response>
        /// <response code="400">Invalid input or username already exists</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for username: {Username}", registerDto.Username);

                var result = await _authService.RegisterAsync(registerDto);

                _logger.LogInformation("Registration successful for username: {Username}", registerDto.Username);

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request during registration: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", registerDto.Username);
                return StatusCode(500, new ApiResponse(false, "An error occurred during registration"));
            }
        }
    }
}
