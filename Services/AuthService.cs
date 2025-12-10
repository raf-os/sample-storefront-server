using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;

namespace SampleStorefront.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwtTokenService;
    public AuthService(AppDbContext db, JwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public class RefreshTokenResponse
    {
        public required string RefreshToken { get; set; }
        public required string JWTToken { get; set; }
    }

    public async Task<RefreshTokenResponse?> GenerateRefreshToken(User user)
    {
        var refreshToken = Guid.NewGuid().ToString();
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _db.SaveChangesAsync();

        var jwttoken = _jwtTokenService.GenerateToken(user);

        return new RefreshTokenResponse
        {
            RefreshToken = refreshToken,
            JWTToken = jwttoken
        };
    }

    public async Task InvalidateExistingTokens(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync();
    }
}