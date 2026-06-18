using System.Text.Json.Serialization;

namespace MainSolutions.API.Models;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = [];
}
