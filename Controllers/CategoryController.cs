using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Services;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CategoryService _categoryService;
    public CategoryController(AppDbContext db, CategoryService categoryService)
    {
        _db = db;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetCategoryTree();

        if (categories == null)
            return NotFound();

        return Ok(new { Categories = categories });
    }
}