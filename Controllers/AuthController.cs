using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;

[ApiController]
[Route("auth/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;
    public record LoginRequest(string Username, string Password);
    public record RefreshRequest(string RefreshToken);

    public AuthController(AppDbContext db, PasswordService passwordService, JwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    private async Task<string> GenerateRefreshToken(Guid userId)
    {
        var refreshToken = Guid.NewGuid().ToString();
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = userId.ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _db.SaveChangesAsync();

        return refreshToken;
    }

    [HttpPost("login")]
    public async Task<IActionResult> UserLogin(LoginRequest request)
    {
        var username = request.Username;
        var password = request.Password;

        var user = await _db.Users.Where(u => u.Name == username).FirstOrDefaultAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        bool isValid = _passwordService.CheckHashedPassword(password, user.Password);

        if (!isValid)
        {
            return Unauthorized();
        }

        var refreshToken = await GenerateRefreshToken(user.Id);

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        var token = _jwtTokenService.GenerateToken(user.Id.ToString(), username);

        return Ok(new { token, refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> TokenRefresh(RefreshRequest request)
    {
        var requestToken = request.RefreshToken;

        var tokenLookup = await _db.RefreshTokens
            .Where(t => t.Token == requestToken)
            .SingleOrDefaultAsync();

        if (tokenLookup == null || tokenLookup.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var user = await _db.Users
            .Where(u => u.Id.ToString() == tokenLookup.UserId)
            .SingleOrDefaultAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        var newToken = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Name);

        return Ok(new { token = newToken });
    }

    [HttpPost("register")]
    public async Task<IActionResult> UserRegister()
    {
        return NotFound();
    }
}