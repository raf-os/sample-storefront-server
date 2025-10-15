namespace SampleStorefront.Models;

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required float Price { get; set; }
    public float? Discount { get; set; }
    public string? Description { get; set; }
    public int? Rating { get; set; }
    public List<Comment> Comments { get; } = [];
}