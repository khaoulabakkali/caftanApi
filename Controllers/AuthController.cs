using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Le login et le mot de passe sont requis"
            });
        }

        var result = await _authService.LoginAsync(request.Login, request.Password);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NomComplet) || 
            string.IsNullOrWhiteSpace(request.Login) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Le nom complet, le login et le mot de passe sont requis"
            });
        }

        var success = await _authService.RegisterAsync(
            request.NomComplet, 
            request.Login, 
            request.Password, 
            request.IdRole, 
            request.Telephone);

        if (!success)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Le login existe déjà"
            });
        }

        return Ok(new LoginResponse
        {
            Success = true,
            Message = "Inscription réussie"
        });
    }
}

