using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    public UserController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{Id:guid}")]
    [ProducesResponseType<UserPublicDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchUserProfile(Guid Id)
    {
        var user = await _db.Users
            .Where(x => x.Id == Id)
            .Include(u => u.Products
                .OrderBy(x => x.CreationDate)
                .Take(5))
            .Include(u => u.Comments
                .OrderBy(x => x.PostDate)
                .Take(5))
            .SingleOrDefaultAsync();

        if (user == null)
            return NotFound();

        var userDTO = new UserPublicDTO(user)
            .WithComments([.. user.Comments])
            .WithProducts([.. user.Products]);

        return Ok(userDTO);
    }

    [Authorize]
    [HttpGet("my-data")]
    [ProducesResponseType<UserDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FetchUserPrivateData()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _db.Users
            .Where(x => x.Id == userGuid)
            .SingleOrDefaultAsync();
        
        if (user == null)
            return Unauthorized();

        var userDTO = new UserDTO(user);

        return Ok(userDTO);
    }
}