using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _db.Categories.ToListAsync();

        return Ok(categories);
    }
}