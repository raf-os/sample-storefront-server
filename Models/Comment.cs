namespace SampleStorefront.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string? Content { get; set; }
    public int? Score { get; set; }

    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public required Guid UserId { get; set; }
    public required User User { get; set; }
}

public class CommentDTO
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public int? Score { get; set; }

    public Guid ProductId { get; set; }
    public ProductDTO Product { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserDTO User { get; set; } = null!;

    public CommentDTO() { }
    public CommentDTO(Comment c)
    {
        Id = c.Id;
        Content = c.Content;
        Score = c.Score;

        ProductId = c.ProductId;
        Product = new ProductDTO(c.Product);

        UserId = c.UserId;
        User = new UserDTO(c.User);
    }
}