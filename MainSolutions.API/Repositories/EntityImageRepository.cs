using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

/// <summary>
/// Extends BaseRepository so the standard CRUD operations (CreateAsync,
/// UpdateAsync, DeleteAsync, GetByIdAsync) are inherited rather than
/// re-implemented. Image-specific queries are added here.
/// </summary>
public class EntityImageRepository : BaseRepository<EntityImage>, IEntityImageRepository
{
    public EntityImageRepository(AppDbContext context) : base(context)
    {
    }

    // ── IEntityImageRepository-specific queries ────────────────────────────

    public async Task<IReadOnlyList<EntityImage>> GetByEntityAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(i => i.EntityType == entityType && i.EntityId == entityId)
            .OrderBy(i => i.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<EntityImage> AddAsync(EntityImage image, CancellationToken cancellationToken = default)
        // BaseRepository.CreateAsync handles Add + SaveChanges.
        => await CreateAsync(image, cancellationToken);

    public async Task DeleteAllForEntityAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var images = await _dbSet
            .Where(i => i.EntityType == entityType && i.EntityId == entityId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(images);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(
        IEnumerable<(int Id, int SortOrder)> updates, CancellationToken cancellationToken = default)
    {
        foreach (var (id, sortOrder) in updates)
        {
            // Use base GetByIdAsync (FindAsync) to avoid extra queries.
            var image = await GetByIdAsync(id, cancellationToken);
            if (image is not null)
                image.SortOrder = sortOrder;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}