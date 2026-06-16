using System.Text.Json.Serialization;

namespace MainSolutions.API.Models;

// MainSolutions.API/Models/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }

    /// <summary>Public URL of the product image in blob storage, if any.</summary>
    public string? ImagePath { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Category? Category { get; set; }
}
