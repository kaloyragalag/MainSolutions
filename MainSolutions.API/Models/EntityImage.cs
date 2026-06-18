namespace MainSolutions.API.Models;

/// <summary>
/// Polymorphic image record. <see cref="EntityType"/> identifies which table the image
/// belongs to (e.g. "product"), and <see cref="EntityId"/> is the PK of that row.
/// Inherits Id, IsActive, CreatedAt, and UpdatedAt from <see cref="BaseEntity"/>.
/// </summary>
public class EntityImage : BaseEntity
{
    /// <summary>Lowercase entity name, e.g. "product".</summary>
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    /// <summary>Public URL returned from blob storage.</summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Display order (0-based). Lower values appear first.</summary>
    public int SortOrder { get; set; }
}
