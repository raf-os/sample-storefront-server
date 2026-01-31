namespace SampleStorefront.Models;

public class ProductCategory
{
  public Guid ProductId { get; set; }
  public int CategoryId { get; set; }

  public Product Product { get; set; } = null!;
  public Category Category { get; set; } = null!;
}
