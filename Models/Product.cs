namespace SampleStorefront.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required float Price { get; set; }
    public float? Discount { get; set; }
    public string? Description { get; set; }
    public int? Rating { get; set; }
    public List<Comment> Comments { get; } = [];
}