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
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
  private readonly int _pageSize = 6;
  private readonly AppDbContext _db;
  public record PostCommentRequest(float Rating, string Content);

  public CommentController(AppDbContext db)
  {
    _db = db;
  }

  public async Task<bool> UpdateScore(Product product)
  {
    // Todo: set this up as a background task or something
    var query = await _db.Comments
        .Where(c => c.ProductId == product.Id)
        .GroupBy(c => c.ProductId)
        .Select(x => new
        {
          AverageScore = x.Average(c => c.Score),
          Count = x.Count()
        })
        .FirstOrDefaultAsync();

    if (query == null)
      return false;

    var score = new ProductRating { Amount = query.Count, Value = query.AverageScore };

    product.Rating = score;
    await _db.SaveChangesAsync();

    return true;
  }

  public class FetchCommentsResponse
  {
    public List<CommentDTO> Comments { get; set; } = [];
    public bool HasCommented { get; set; } = false;
    public bool IsEndOfList { get; set; }
  }

  [HttpGet("{Id}")]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType<FetchCommentsResponse>(StatusCodes.Status200OK)]
  public async Task<IActionResult> FetchComments(Guid Id, Guid lastId, DateTimeOffset lastDate)
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub!);
    var successfulParse = Guid.TryParse(userId, out var userGuid);

    var query = _db.Comments
        .Where(x => x.ProductId == Id)
        .Include(x => x.User)
        .AsQueryable();

    bool hasCommented = successfulParse == true && await query.AnyAsync(c => c.UserId == userGuid);

    var comments = await query
        .OrderBy(x => x.PostDate)
        .Where(x => x.PostDate > lastDate || (x.PostDate == lastDate && x.Id != lastId))
        .Take(_pageSize)
        .Select(x => new CommentDTO(x).WithUser(x.User))
        .ToListAsync();

    if (comments == null)
      return NotFound();

    var isEndOfList = comments.Count < _pageSize;

    return Ok(new FetchCommentsResponse
    {
      Comments = comments,
      HasCommented = hasCommented,
      IsEndOfList = isEndOfList
    });
  }

  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> AddComment(Guid Id, [FromBody] PostCommentRequest request)
  {
    var content = request.Content;
    var score = request.Rating;

    if (score < 0f || score > 5f)
    {
      return BadRequest(new { Message = "Score must be between 0 and 5." });
    }

    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var query = _db.Products
        .Where(p => p.Id == Id)
        .AsQueryable();

    var hasCommented = await query.Include(p => p.Comments).AnyAsync(c => c.UserId == userGuid);

    if (hasCommented == true)
      return Unauthorized();

    var product = await query
        .FirstOrDefaultAsync();

    if (product == null)
      return NotFound();

    var user = await _db.Users.Where(u => u.Id == userGuid).FirstOrDefaultAsync();
    if (user == null || user.IsVerified == false)
      return Unauthorized();

    var _comment = new Comment(content, score, product, user);

    _db.Comments.Add(_comment);

    await _db.SaveChangesAsync();

    await UpdateScore(product);

    return Created();
  }

  [Authorize]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteComment(Guid Id)
  {
    var comment = await _db.Comments.Where(c => c.Id == Id).FirstOrDefaultAsync();

    if (comment == null)
      return NotFound();

    var pId = comment.ProductId;

    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    if (comment.UserId.ToString() != userId)
      return Unauthorized();

    _db.Remove(comment);

    await _db.SaveChangesAsync();

    var product = await _db.Products.Where(p => p.Id == pId).FirstOrDefaultAsync();
    if (product != null)
      await UpdateScore(product);

    return NoContent();
  }
}
