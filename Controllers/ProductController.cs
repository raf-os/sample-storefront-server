using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;

[ApiController]
[Route("api/products/[controller]")]
public class ProductController : ControllerBase
{
    private readonly int _pageSize = 10;
    private readonly AppDbContext _db;
    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("page")]
    public async Task<IActionResult> FetchPage([Range(0, int.MaxValue)] int offset = 0)
    {
        var items = await _db.Products
            .OrderBy(x => x.CreationDate)
            .Skip(offset * _pageSize)
            .Take(_pageSize)
            .Select(i => new ProductListItemDTO(i))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("item")]
    public async Task<IActionResult> FetchItem(Guid id)
    {
        var item = await _db.Products
            .Where(x => x.Id == id)
            .Select(i => new ProductDTO(i))
            .SingleOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(item);
        }
    }
}