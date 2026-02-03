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
[Authorize]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
  private readonly int MAX_MAILS_PER_PAGE = 10;
  private readonly AppDbContext _db;
  public MailController(AppDbContext db)
  {
    _db = db;
  }

  public class UserMailFetchFilter
  {
    [Range(1, int.MaxValue)]
    public int Offset { get; set; } = 1;
    public Guid? Source { get; set; }
  }

  public class SendMailRequest
  {
    public string? Title { get; set; }
    [Required]
    public required string Content { get; set; }
  }

  [HttpGet("inbox")]
  [ProducesResponseType<List<MailPreviewDTO>>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetUserMail(
      [FromQuery] UserMailFetchFilter filter)
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var query = _db.Mails
        .Where(x => x.RecipientId == userGuid)
        .Include(x => x.Sender)
        .AsQueryable();

    query = query
        .OrderByDescending(x => x.SendDate)
        .Skip((filter.Offset - 1) * MAX_MAILS_PER_PAGE)
        .Take(MAX_MAILS_PER_PAGE);

    var result = await query
        .Select(x => new MailPreviewDTO(x))
        .ToListAsync();

    return Ok(result);
  }

  [HttpGet("inbox/preview")]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<List<MailPreviewDTO>>(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetUnreadMailPreview()
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var result = await _db.Mails
        .Where(x => x.RecipientId == userGuid)
        .Where(x => x.IsRead == false)
        .Include(x => x.Sender)
          .ThenInclude(u => u.Avatar)
        .OrderByDescending(x => x.SendDate)
        .Take(5)
        .Select(x => new MailPreviewDTO(x))
        .ToListAsync();

    return Ok(result);
  }

  [HttpGet("inbox/{Id:guid}")]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType<MailDTO>(StatusCodes.Status200OK)]
  public async Task<IActionResult> ViewMail(
      [FromRoute] Guid Id
      )
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var result = await _db.Mails
        .Where(x => x.Id == Id && x.RecipientId == userGuid)
        .Include(x => x.Sender)
        .SingleOrDefaultAsync();

    if (result == null)
      return NotFound();

    result.IsRead = true;

    await _db.SaveChangesAsync();

    var mailData = new MailDTO(result);

    return Ok(mailData);
  }

  [HttpPost("send/{Id:guid}")]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> SendMailTo(
      [FromRoute] Guid Id,
      [FromBody] SendMailRequest mailRequest
      )
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var doesRecipientExist = await _db.Users
        .Where(x => x.Id == Id)
        .AnyAsync();

    if (doesRecipientExist == false)
      return NotFound();

    var mail = new Mail
    {
      Title = mailRequest.Title,
      Content = mailRequest.Content,

      SenderId = userGuid,
      RecipientId = Id,
    };

    _db.Mails.Add(mail);

    await _db.SaveChangesAsync();

    return NoContent();
  }

  [HttpGet("inbox/size")]
  [ProducesResponseType<int>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetInboxSize(
      [FromQuery] bool unreadOnly = false
      )
  {
    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userId, out var userGuid))
      return Unauthorized();

    var query = _db.Mails
        .Where(x => x.RecipientId == userGuid)
        .AsQueryable();

    if (unreadOnly == true)
      query = query.Where(x => x.IsRead == false);

    var inboxSize = await query
        .CountAsync();

    return Ok(inboxSize);
  }
}
