using System.Text.Json.Serialization;

namespace SampleStorefront.Models;

public class CartItem
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;

  public Guid ProductId { get; set; }
  public Product Product { get; set; } = null!;

  public int Quantity { get; set; } = 1;
  public float PriceSnapshot { get; set; }
  public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class CartItemDTO
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public UserDTO? User { get; set; }

  public Guid ProductId { get; set; }
  public ProductListItemDTO? Product { get; set; }

  public int Quantity { get; set; } = 1;
  public DateTime AddedAt { get; set; } = DateTime.UtcNow;
  public CartItemDTO() { }
}
