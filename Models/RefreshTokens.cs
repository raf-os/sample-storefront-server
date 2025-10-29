namespace SampleStorefront.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}