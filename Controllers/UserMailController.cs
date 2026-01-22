using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserMailController : ControllerBase
{
    private readonly int MAX_MAILS_PER_PAGE = 10;
    private readonly AppDbContext _db;
    public UserMailController(AppDbContext db)
    {
        _db = db;
    }

    public class UserMailFetchFilter
    {
        [Range(1, int.MaxValue)]
        public int Offset { get; set; } = 1;
        public Guid? Source { get; set; }
    }

    [HttpGet("user/{Id:guid}")]
    public async Task<IActionResult> GetUserMail(
        Guid Id,
        [FromQuery] UserMailFetchFilter filter
        )
    {
        var query = _db.Mails
            .Where(x => x.RecipientId == Id)
            .Include(x => x.Sender)
            .AsQueryable();

        query = query
            .OrderBy(x => x.SendDate)
            .Skip((filter.Offset - 1) * MAX_MAILS_PER_PAGE)
            .Take(MAX_MAILS_PER_PAGE);

        return Ok();
    }
}
