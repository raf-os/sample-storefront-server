using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;

[ApiController]
[Route("api/products/[controller]")]
public class ProductController : ControllerBase
{
    public record NewProductRequest(string Name, float Price, string? Description);
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

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> NewItem(NewProductRequest product)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var user = await _db.Users.Where(u => u.Id.ToString() == userId).SingleOrDefaultAsync();
        if (user == null || user.IsVerified == false)
        {
            return Unauthorized();
        }

        _db.Products.Add(new Product
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description
        });

        await _db.SaveChangesAsync();

        return Created();
    }

    [Authorize]
    [HttpDelete("item")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var user = await _db.Users.Where(u => u.Id.ToString() == userId).SingleOrDefaultAsync();
        if (user == null || user.IsVerified == false)
        {
            return Unauthorized();
        }

        var productToRemove = await _db.Products.Where(p => p.Id == id).SingleOrDefaultAsync();

        if (productToRemove == null)
        {
            return NotFound();
        }

        _db.Products.Remove(productToRemove);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}