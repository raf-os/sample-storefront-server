using System.Linq.Expressions;

namespace SampleStorefront.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime PostDate { get; set; } = DateTime.UtcNow;

    public string? Content { get; set; }
    public float Score { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Comment() { }
    public Comment(string content, float score, Product product, User user)
    {
        Content = content;
        Score = score;
        //Product = product ?? throw new ArgumentNullException(nameof(product));
        ProductId = product.Id;
        //User = user ?? throw new ArgumentNullException(nameof(user));
        UserId = user.Id;
    }
}

public class CommentDTO
{
    public Guid Id { get; set; }
    public DateTime PostDate { get; set; }
    public string? Content { get; set; }
    public float Score { get; set; }

    public ProductDTO? Product { get; set; }

    public Guid UserId { get; set; }
    public UserPublicDTO? User { get; set; }

    public CommentDTO() { }
    public CommentDTO(Comment c)
    {
        Id = c.Id;
        PostDate = c.PostDate;
        Content = c.Content;
        Score = c.Score;
    }

    public CommentDTO WithUser(User u)
    {
        User = new UserPublicDTO(u);
        UserId = User.Id;
        return this;
    }

    public CommentDTO WithProduct(Product p)
    {
        Product = new ProductDTO(p);
        return this;
    }
}