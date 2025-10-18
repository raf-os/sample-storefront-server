namespace SampleStorefront.Models;

public enum UserRole
{
    User,
    Operator,
    Admin,
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public List<Comment> Comments { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}

public class UserDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UserRole Role { get; set; }

    public List<CommentDTO>? Comments { get; set; }
    public List<ProductDTO>? Products { get; set; }

    public UserDTO() { }
    public UserDTO(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        Role = user.Role;

        if (user.Comments != null)
        {
            Comments = user.Comments.Select(c => new CommentDTO(c)).ToList();
        }

        if (user.Products != null)
        {
            Products = user.Products.Select(p => new ProductDTO(p)).ToList();
        }
    }
}