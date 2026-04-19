using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Login and get JWT token</summary>
    [HttpPost("login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Logout and revoke current token</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { success = false, message = "No token provided." });

        await _authService.LogoutAsync(token);
        return Ok(new { success = true, message = "Logged out successfully." });
    }
}
