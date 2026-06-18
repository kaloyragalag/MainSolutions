using MainSolutions.API.Models;
using MainSolutions.API.DTOs;

namespace MainSolutions.API.Repositories.Interfaces;

/// <summary>
/// Extends IBaseRepository so GetByIdAsync, DeleteAsync, CreateAsync,
/// UpdateAsync, and ExistsAsync are inherited — only image-specific
/// operations are declared here.
/// </summary>
public interface IEntityImageRepository : IBaseRepository<EntityImage>
{
    /// <summary>All images for a given entity, ordered by SortOrder.</summary>
    Task<IReadOnlyList<EntityImage>> GetByEntityAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience alias for CreateAsync so call-sites read clearly
    /// (imageRepository.AddAsync vs imageRepository.CreateAsync).
    /// </summary>
    Task<EntityImage> AddAsync(EntityImage image, CancellationToken cancellationToken = default);

    /// <summary>Removes every image row for the given entity in one batch.</summary>
    Task DeleteAllForEntityAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>Bulk-updates SortOrder values without reloading the full list.</summary>
    Task ReorderAsync(
        IEnumerable<(int Id, int SortOrder)> updates, CancellationToken cancellationToken = default);
}
