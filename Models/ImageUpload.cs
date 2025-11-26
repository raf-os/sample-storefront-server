namespace SampleStorefront.Models;

public class ImageUpload
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public string Url { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid UploaderId { get; set; }
}