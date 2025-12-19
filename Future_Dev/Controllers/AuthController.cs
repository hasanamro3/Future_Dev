using Application.DTOs.Auth.Login;
using Application.DTOs.Auth.Password;
using Application.DTOs.Auth.Register;
using Application.Service.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto input)
    {
        var response = await _authService.LoginAsync(input);
        if (response == null)
            return Unauthorized("Invalid email or password.");

        return Ok(response);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto input)
    {
        var response = await _authService.RegisterAsync(input);
        if (response == null)
            return BadRequest("Cannot Register.");

        return Ok(response);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto input)
    {
        await _authService.ResetPassword(input);
        return Ok();
    }

}
