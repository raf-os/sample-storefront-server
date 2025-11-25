using Microsoft.EntityFrameworkCore;

namespace SampleStorefront.Models;

[Owned]
public class ProductRating
{
    public float? Value { get; set; }
    public int Amount { get; set; } = 0;
}

[Owned]
public class ProductMetadata
{
    public int Sales { get; set; } = 0;
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
    public List<string> Tags { get; set; } = [];
    public ProductMetadata Metadata { get; set; } = new ProductMetadata();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];
}

public class ProductListItemDTO
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Name { get; set; } = null!;
    public float Price { get; set; }
    public float? Discount { get; set; }
    public ICollection<CategoryDTO> Categories { get; set; } = [];

    public ProductListItemDTO() { }

    public ProductListItemDTO(Product p)
    {
        Id = p.Id;
        CreationDate = p.CreationDate;
        Name = p.Name;
        Price = p.Price;
        Discount = p.Discount;

        Categories = p.ProductCategories
            .Select(pc => new CategoryDTO(pc.Category))
            .ToList();
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
    public ProductRating Rating { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
    public ProductMetadata Metadata { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserPublicDTO? User { get; set; } = null!;

    public ICollection<CommentDTO>? Comments { get; set; }
    // public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    public ICollection<CategoryDTO> Categories { get; set; } = [];

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
        Tags = p.Tags;
        Metadata = p.Metadata;

        UserId = p.UserId;
        if (p.User != null)
            User = new UserPublicDTO(p.User);

        if (p.Comments != null)
        {
            Comments = p.Comments.Select(c => new CommentDTO(c)).ToList();
        }

        Categories = p.ProductCategories
            .Select(pc => new CategoryDTO(pc.Category))
            .ToList();
    }
}

public class ProductPatchDTO
{
    //public Guid Id { get; }
    public string? Name { get; set; }
    public float? Price { get; set; }
    public float? Discount { get; set; }
    public string? Description { get; set; }
    public List<int> Categories { get; set; } = [];
    // public Guid UserId { get; }

    public ProductPatchDTO() { }
    public ProductPatchDTO(Product p)
    {
        //Id = p.Id;
        Name = p.Name;
        Price = p.Price;
        Discount = p.Discount;
        Description = p.Description;
        // UserId = p.UserId;
    }
}