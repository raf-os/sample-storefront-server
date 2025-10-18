using Microsoft.EntityFrameworkCore;

namespace SampleStorefront.Models;

[Owned]
public class ProductRating
{
    public float? Value;
    public int Amount = 0;
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public required string Name { get; set; }
    public required float Price { get; set; }
    public float? Discount { get; set; }
    public string? Description { get; set; }
    public ProductRating Rating { get; set; } = new ProductRating();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public List<Comment> Comments { get; set; } = [];
}

public class ProductListItemDTO
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Name { get; set; } = default!;
    public float Price { get; set; }
    public float? Discount { get; set; }

    public ProductListItemDTO() { }

    public ProductListItemDTO(Product p)
    {
        Id = p.Id;
        CreationDate = p.CreationDate;
        Name = p.Name;
        Price = p.Price;
        Discount = p.Discount;
    }
}

public class ProductDTO
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Name { get; set; } = default!;
    public float Price { get; set; }
    public float? Discount { get; set; }
    public string? Description { get; set; }
    public ProductRating Rating { get; set; }

    public Guid UserId { get; set; }
    public UserDTO User { get; set; } = null!;

    public List<CommentDTO>? Comments { get; set; }

    public ProductDTO() { }
    public ProductDTO(Product p)
    {
        Id = p.Id;
        CreationDate = p.CreationDate;
        Name = p.Name;
        Price = p.Price;
        Discount = p.Discount;
        Description = p.Description;
        Rating = p.Rating;

        UserId = p.UserId;
        User = new UserDTO(p.User);

        if (p.Comments != null)
        {
            Comments = p.Comments.Select(c => new CommentDTO(c)).ToList();
        }
    }
}