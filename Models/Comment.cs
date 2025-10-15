namespace SampleStorefront.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public string? Content { get; set; }
    public int? Score { get; set; }

    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }
}