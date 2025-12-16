using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = [];
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = [];
    [JsonIgnore]
    public ICollection<CartItem> CartItems { get; set; } = [];
    [JsonIgnore]
    public UserAvatar? Avatar { get; set; }
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
    public int CartItemAmount { get; set; } = 0;

    public ICollection<CommentDTO>? Comments { get; set; }
    public ICollection<ProductDTO>? Products { get; set; }
    public UserAvatar? Avatar { get; set; }

    public UserDTO() { }
    public UserDTO(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;

        SignupDate = user.SignupDate;
        IsVerified = user.IsVerified;

        Role = user.Role;

        // if (user.Comments != null)
        // {
        //     Comments = user.Comments.Select(c => new CommentDTO(c)).ToList();
        // }

        // if (user.Products != null)
        // {
        //     Products = user.Products.Select(p => new ProductDTO(p)).ToList();
        // }
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
    public Guid? AvatarId { get; set; }

    public UserPublicDTO() { }
    public UserPublicDTO(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Role = user.Role;
        SignupDate = user.SignupDate;
    }

    public UserPublicDTO WithComments(List<Comment> c)
    {
        Comments = c.Select(c => new CommentDTO(c)).ToList();
        return this;
    }

    public UserPublicDTO WithProducts(List<Product> p)
    {
        Products = p.Select(p => new ProductDTO(p)).ToList();
        return this;
    }
}