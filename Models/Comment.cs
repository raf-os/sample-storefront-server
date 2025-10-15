namespace SampleStorefront.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Content { get; set; }
    public int? Score { get; set; }

    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public Comment(Guid _userId)
    {
        Id = Guid.NewGuid();
        UserId = _userId;
    }
}