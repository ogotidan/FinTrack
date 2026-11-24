using FinTrack.Api.Helpers;
using FinTrack.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DBHandler _db;
    private readonly JwtHelper _jwt;

    public AuthController(DBHandler db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        var name = req.Name.Trim();
        var email = req.Email.Trim().ToLowerInvariant();

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12);
        var id = _db.RegisterAnalyst(name, email, hash);
        if (id == 0)
            return Conflict(new ApiError("An account with that email already exists."));

        var (token, expires) = _jwt.GenerateToken(id, name, email);
        var body = new AuthResponse
        {
            Token = token, AnalystId = id, Name = name, Email = email, ExpiresAt = expires
        };
        return Created(string.Empty, ApiResponse<AuthResponse>.Ok(body, "Registration successful."));
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var analyst = _db.ValidateLogin(email);

        if (analyst is null || !analyst.IsActive ||
            !BCrypt.Net.BCrypt.Verify(req.Password, analyst.PasswordHash))
        {
            return Unauthorized(new ApiError("Invalid email or password."));
        }

        _db.UpdateLastLogin(analyst.Id);
        var (token, expires) = _jwt.GenerateToken(analyst.Id, analyst.Name, analyst.Email);
        var body = new AuthResponse
        {
            Token = token, AnalystId = analyst.Id, Name = analyst.Name,
            Email = analyst.Email, ExpiresAt = expires
        };
        return Ok(ApiResponse<AuthResponse>.Ok(body, "Login successful."));
    }
}
