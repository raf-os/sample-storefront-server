namespace SampleStorefront.Models;

public class UserAvatar
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTime UploadDate { get; set; } = DateTime.UtcNow;
  public string Url { get; set; } = null!;
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
}
