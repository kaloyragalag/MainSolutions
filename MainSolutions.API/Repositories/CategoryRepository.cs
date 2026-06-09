using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(string name)
        => await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());

    public async Task<bool> ExistsAsync(string name, int excludeId)
        => await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId);

    protected override IQueryable<Category> ApplySearch(IQueryable<Category> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var term = search.ToLower();
        return query.Where(c =>
            c.Name.ToLower().Contains(term) ||
            c.Description.ToLower().Contains(term)
        );
    }
}
