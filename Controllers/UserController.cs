using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;
using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using FileTypeChecker.Extensions;
using FileTypeChecker;

namespace SampleStorefront.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;
    public UserController(AppDbContext db, PasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }
    public class UserUpdateSchemaRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        public required string Password { get; set; }

        [MinLength(4, ErrorMessage = "New password must be at least 4 characters long.")]
        [MaxLength(40, ErrorMessage = "New password must be at most 40 characters long.")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "The new passwords do not match.")]
        public string? NewPasswordConfirm { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
    }

    public class ProfilePicUploadRequest
    {
        [Required(ErrorMessage = "A file is required.")]
        public required IFormFile Image { get; set; }
    }

    private async Task<string> SaveImageToWebP(IFormFile file)
    {
        // TODO: Move this to a service
        const long maxFileSize = 5 * 1024 * 1024;
        var allowedExtensions = new[] { "PNG", "JPEG", "JPG", "WEBP" };

        if (file.Length > maxFileSize)
            throw new BadHttpRequestException($"File {file.FileName} exceeds maximum allowed size of 5MB.");

        using (var stream = file.OpenReadStream())
        {
            if (!stream.IsImage())
                throw new BadHttpRequestException("Only image files are allowed.");

            var fileType = FileTypeValidator.GetFileType(stream);

            if (!allowedExtensions.Contains(fileType.Name))
                throw new BadHttpRequestException($"Invalid image type for file {file.FileName}.");

            file.OpenReadStream().Position = 0;
        }

        Size finalSize = new(0, 0);
        var dirPath = Path.Combine("Uploads", "Profiles");
        Directory.CreateDirectory(dirPath);
        var thumbPath = Path.Combine("Uploads", "Thumbnails", "Profiles");
        Directory.CreateDirectory(thumbPath);

        var fileGuid = Guid.NewGuid();
        var fileName = $"{fileGuid}.webp";
        var filePath = Path.Combine(dirPath, fileName);
        var fileThumbPath = Path.Combine(thumbPath, fileName);

        using (var image = await Image.LoadAsync(file.OpenReadStream()))
        {
            image.Mutate(x => x.AutoOrient());

            if (image.Width > 256 || image.Height > 256)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(256, 256),
                    Mode = ResizeMode.Max
                }));
            }

            image.Metadata.ExifProfile = null;

            var thumbClone = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(48, 48),
                Mode = ResizeMode.Max
            }));

            var encoder = new WebpEncoder
            {
                Quality = 85,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(filePath, encoder);
            await thumbClone.SaveAsync(fileThumbPath, encoder);
        }

        return fileName;
    }
    
    private static void DeleteAvatarByName(string imageName)
    {
        var filePath = Path.Combine("Uploads", "Profiles", imageName);
        var thumbPath = Path.Combine("Uploads", "Thumbnails", "Profiles", imageName);

        if (Path.Exists(filePath))
            System.IO.File.Delete(filePath);
        
        if (Path.Exists(thumbPath))
            System.IO.File.Delete(thumbPath);
    }

    [HttpGet("{Id:guid}")]
    [ProducesResponseType<UserPublicDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchUserProfile(Guid Id, bool comments = false, bool products = false)
    {
        var query = _db.Users
            .Where(x => x.Id == Id)
            .Include(x => x.Avatar)
            .AsQueryable();

        if (products == true)
            query.Include(u => u.Products
                .OrderBy(x => x.CreationDate)
                .Take(5));

        if (comments == true)
            query.Include(u => u.Comments
                .OrderBy(x => x.PostDate)
                .Take(5));

        var user = await query
            .SingleOrDefaultAsync();

        if (user == null)
            return NotFound();

        var userDTO = new UserPublicDTO(user)
            .WithComments([.. user.Comments])
            .WithProducts([.. user.Products]);

        return Ok(userDTO);
    }

    [Authorize]
    [HttpGet("my-data")]
    [ProducesResponseType<UserDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FetchUserPrivateData()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _db.Users
            .Where(x => x.Id == userGuid)
            .Include(x => x.Avatar)
            .SingleOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        var userDTO = new UserDTO(user);

        return Ok(userDTO);
    }

    [Authorize]
    [HttpPost("update-profile")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserProfile(UserUpdateSchemaRequest request)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _db.Users
            .Where(x => x.Id == userGuid)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        var oldPassword = request.Password;

        if (!_passwordService.CheckHashedPassword(oldPassword, user.Password))
            return Unauthorized("Incorrect credentials.");

        if (request.NewPassword != null)
        {
            var hashedPw = _passwordService.HashPassword(request.NewPassword);
            if (hashedPw == null)
                return BadRequest("Error hashing password. Try a different one.");
            user.Password = hashedPw;
        }

        if (request.Email != null)
            user.Email = request.Email;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPost("profile-pic")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UploadUserProfilePic([FromForm] ProfilePicUploadRequest request)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _db.Users
            .Where(x => x.Id == userGuid)
            .Include(x => x.Avatar)
            .SingleOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        var imageFile = request.Image;

        var imageName = await SaveImageToWebP(imageFile);

        var avatar = new UserAvatar
        {
            Url = imageName,
            UserId = userGuid
        };

        if (user.Avatar != null)
        {
            _db.UserAvatars.Remove(user.Avatar);
            DeleteAvatarByName(user.Avatar.Url);
        }

        _db.UserAvatars.Add(avatar);
        user.Avatar = avatar;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("profile-pic")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveProfilePic()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _db.Users
            .Where(x => x.Id == userGuid)
            .Include(x => x.Avatar)
            .SingleOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        if (user.Avatar == null)
            return NoContent();

        _db.UserAvatars.Remove(user.Avatar);
        DeleteAvatarByName(user.Avatar.Url);

        user.Avatar = null;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}