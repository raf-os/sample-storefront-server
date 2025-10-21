using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("api/product/comments")]
public class CommentController : ControllerBase
{
    private readonly int _pageSize = 6;
    private readonly AppDbContext _db;
    public record PostCommentRequest(float Score, string Content);

    public CommentController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FetchComments(Guid Id, [Range(0, int.MaxValue)] int Page = 0)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var query = _db.Comments
            .Where(x => x.ProductId == Id);

        var totalCount = await query.CountAsync();
        var totalPages = MathF.Ceiling((float)totalCount / (float)_pageSize);
        bool hasCommented = userId == null
            ? false
            : await query.AnyAsync(c => c.UserId.ToString() == userId);

        var comments = await query
            .OrderBy(x => x.PostDate)
            .Skip(Page * _pageSize)
            .Take(_pageSize)
            .Select(x => new CommentDTO(x))
            .ToListAsync();

        if (comments == null)
            return NotFound();

        return Ok(new { comments, totalPages, hasCommented });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> AddComment(Guid Id, [FromBody] PostCommentRequest request)
    {
        var content = request.Content;
        var score = request.Score;

        if (score < 0f || score > 5f)
        {
            return BadRequest(new { Message = "Score must be between 0 and 5." });
        }

        var product = await _db.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var user = await _db.Users.Where(u => u.Id.ToString() == userId).FirstOrDefaultAsync();
        if (user == null || user.IsVerified == false)
            return Unauthorized();

        var _comment = new Comment(content, score, product, user);

        _db.Comments.Add(_comment);

        await _db.SaveChangesAsync();

        return Created();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid Id)
    {
        var comment = await _db.Comments.Where(c => c.Id == Id).FirstOrDefaultAsync();

        if (comment == null)
            return NotFound();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

        if (comment.UserId.ToString() != userId)
            return Unauthorized();

        _db.Remove(comment);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}