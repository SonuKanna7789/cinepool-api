using CinePool.API.DTOs;
using CinePool.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Created("/api/profile", result);
    }

    /// <summary>Login and receive JWT tokens</summary>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login and receive JWT tokens", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>Refresh access token using a valid refresh token</summary>
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresh access token", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        return Ok(result);
    }
}
