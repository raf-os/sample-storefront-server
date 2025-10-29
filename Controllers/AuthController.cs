using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;
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
            UserId = userId,
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

        try
        {
            var refreshToken = await GenerateRefreshToken(user.Id);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            var token = _jwtTokenService.GenerateToken(user);
            return Ok(token);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
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
}