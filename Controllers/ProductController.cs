using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    public class NewProductRequest
    {
        public required string Name { get; set; }
        public required float Price { get; set; }
        public string? Description { get; set; }
        public List<int>? Categories { get; set; }
    }
    private readonly int _pageSize = 10;
    private readonly AppDbContext _db;
    private readonly CategoryService _categoryService;
    public ProductController(AppDbContext db, CategoryService categoryService)
    {
        _db = db;
        _categoryService = categoryService;
    }
    public class UpdateItemRequest
    {
        public required JsonPatchDocument<ProductPatchDTO> PatchItem { get; set; }
        public List<int> Categories { get; set; } = [];
    }

    [HttpGet("page")]
    public async Task<IActionResult> FetchPage(
        int? category,
        [Range(0, int.MaxValue)] int offset = 0
        )
    {
        var totalCount = await _db.Products.CountAsync();
        var totalPages = MathF.Ceiling((float)totalCount / (float)_pageSize);
        var query = _db.Products
            .AsQueryable();
        
        if (category != null)
        {
            query = query
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.ProductCategories
                    .Any(pc => pc.CategoryId == category));
        }

        query = query
            .OrderBy(x => x.CreationDate)
            .Skip(offset * _pageSize)
            .Take(_pageSize);

        var result = await query
            .Select(x => new
                {
                    Product = new ProductListItemDTO(x),
                    CommentCount = x.Comments.Count()
                })
            .ToListAsync();
        return Ok(new { items = result, totalPages });
    }

    [HttpGet("item/{id}")]
    public async Task<IActionResult> FetchItem(Guid id)
    {
        // TODO: Also fetch categories
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

        var categoryIds = await _categoryService.ProcessCategoryFromList(product.Categories);

        var prod = new Product
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            UserId = user.Id,
            User = user,
        };

        if (categoryIds != null)
        {
            var catList = new List<ProductCategory>();
            foreach (var id in categoryIds)
            {
                catList.Add(new ProductCategory { CategoryId = id });
            }
            prod.ProductCategories = catList;
        }

        _db.Products.Add(prod);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(FetchPage), new { prod.Id }, new { prod.Id });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        // TODO: Remove categories as well
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

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateItem(Guid Id, [FromBody] UpdateItemRequest request)
    {
        var patchItem = request.PatchItem;

        if (patchItem == null)
        {
            return BadRequest();
        }

        var productToPatch = await _db.Products.Where(p => p.Id == Id).SingleOrDefaultAsync();

        if (productToPatch == null)
            return NotFound();

        var dto = new ProductPatchDTO(productToPatch);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

        if (productToPatch.UserId.ToString() != userId)
            return Unauthorized();
        
        patchItem.ApplyTo(dto, ModelState);

        if (!ModelState.IsValid)
            return BadRequest();

        var categoryIds = await _categoryService.ProcessCategoryFromList(request.Categories);

        var currentCategories = (request.Categories != null) ? (await _db.ProductCategories
            .Where(pc => pc.ProductId == productToPatch.Id)
            .Select(pc => pc.CategoryId)
            .ToListAsync()) : null;
        
        if (currentCategories != null && request.Categories != null)
        {
            var toAdd = request.Categories.Except(currentCategories).ToList();
            var toRemove = currentCategories.Except(request.Categories).ToList();

            if (toRemove.Count != 0)
            {
                var categoriesToRemove = await _db.ProductCategories
                    .Where(pc => pc.ProductId == productToPatch.Id && toRemove.Contains(pc.CategoryId))
                    .ToListAsync();

                _db.ProductCategories.RemoveRange(categoriesToRemove);
            }

            if (toAdd.Count != 0)
            {
                var newCategories = toAdd.Select(categoryId => new ProductCategory
                {
                    ProductId = productToPatch.Id,
                    CategoryId = categoryId
                });

                _db.ProductCategories.AddRange(newCategories);
            }
        }

        if (dto.Name != null) productToPatch.Name = dto.Name;
        if (dto.Price.HasValue) productToPatch.Price = dto.Price.Value;
        if (dto.Discount.HasValue) productToPatch.Discount = dto.Discount.Value;
        if (dto.Description != null) productToPatch.Description = dto.Description;

        await _db.SaveChangesAsync();

        return Ok(dto);
    }
}