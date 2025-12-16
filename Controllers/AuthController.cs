using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;
using SampleStorefront.Settings;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthService _authService;
    private readonly CookieSettings _cookieSettings;
    public record LoginRequest(string Username, string Password);
    public record RegisterRequest
    {
        [StringLength(30, MinimumLength = 3)]
        public string Username { get; init; } = default!;

        [StringLength(50, MinimumLength = 4)]
        public string Password { get; init; } = default!;

        [EmailAddress]
        public string Email { get; init; } = default!;
    }

    public AuthController(AppDbContext db, PasswordService passwordService, JwtTokenService jwtTokenService, AuthService authService, IOptions<CookieSettings> cookieSettings)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _authService = authService;
        _cookieSettings = cookieSettings.Value;
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

        var refreshToken = await _authService.GenerateRefreshToken(user);

        if (refreshToken == null)
            return Unauthorized();

        Response.Cookies.Append("refreshToken", refreshToken.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _cookieSettings.Secure,
            SameSite = _cookieSettings.SameSite,
            Expires = DateTime.UtcNow.AddDays(_cookieSettings.ExpiryDays)
        });

        return Ok(refreshToken.JWTToken);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> TokenRefresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }

        var tokenLookup = await _db.RefreshTokens
            .Where(t => t.Token == refreshToken)
            .SingleOrDefaultAsync();

        if (tokenLookup == null || tokenLookup.ExpiresAt < DateTime.UtcNow)
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized();
        }

        var user = await _db.Users
            .Where(u => u.Id == tokenLookup.UserId)
            .SingleOrDefaultAsync();

        if (user == null)
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized();
        }

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> UserRegister(RegisterRequest request)
    {
        var username = request.Username;
        var password = request.Password;
        var email = request.Email;

        var existingUser = await _db.Users.Where(u => u.Name == username || u.Email == email).FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return Unauthorized(new { Message = "User with either the same name or e-mail already exists." });
        }

        var hashedPassword = _passwordService.HashPassword(password);

        if (hashedPassword == null)
        {
            return BadRequest(new { Message = "Error hashing password. Please select a different one." });
        }

        _db.Users.Add(new User
        {
            Name = username,
            Password = hashedPassword,
            Email = email
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }

        return Created();
    }

    [HttpGet("logout")]
    public async Task<IActionResult> UserLogout()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            // Invalidate existing token
            var existingToken = await _db.RefreshTokens.Where(t => t.Token == refreshToken).SingleOrDefaultAsync();
            if (existingToken != null)
            {
                _db.RefreshTokens.Remove(existingToken);

                await _db.SaveChangesAsync();
            }
        }
        
        Response.Cookies.Delete("refreshToken");

        return NoContent();
    }
}