namespace SampleStorefront.Models;

public class UserAvatar
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTimeOffset UploadDate { get; set; } = DateTimeOffset.UtcNow;
  public string Url { get; set; } = null!;
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
}
