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
    public DateTime SignupDate { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; } = false;
    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}

// Only the user themselves should ever receive this data
public class UserDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UserRole Role { get; set; }
    public DateTime SignupDate { get; set; }
    public bool IsVerified { get; set; }

    public ICollection<CommentDTO>? Comments { get; set; }
    public ICollection<ProductDTO>? Products { get; set; }

    public UserDTO() { }
    public UserDTO(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;

        SignupDate = user.SignupDate;
        IsVerified = user.IsVerified;

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

// This is an user's public data
public class UserPublicDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public UserRole Role { get; set; }
    public DateTime SignupDate { get; set; }
    public ICollection<CommentDTO>? Comments { get; set; }
    public ICollection<ProductDTO>? Products { get; set; }

    public UserPublicDTO() { }
    public UserPublicDTO(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Role = user.Role;
        SignupDate = user.SignupDate;

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