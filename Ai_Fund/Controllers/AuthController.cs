using Ai_Fund.Models;
using Ai_Fund.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ai_Fund.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            if (response == null) return BadRequest(new { message = "Username already exists" });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Registration error", details = ex.Message });
        }
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null) return Unauthorized(new { message = "Invalid username or password" });
        return Ok(response);

    }
}
