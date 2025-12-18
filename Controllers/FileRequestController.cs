using Microsoft.AspNetCore.Mvc;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("files")]
public class FileRequestController : ControllerBase
{
    [HttpGet("avatar/{FileName}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/webp")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/json")]
    public IActionResult GetUserAvatarById(string FileName)
    {
        var filePath = Path.Combine("Uploads", "Profiles", FileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "image/webp");
    }
    
    [HttpGet("thumbnails/product/{FileName}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/webp")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/json")]
    public IActionResult GetProductThumbnailById(string FileName)
    {
        var filePath = Path.Combine("Uploads", "Thumbnails", "Products", FileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();
        
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "image/webp");
    }
}