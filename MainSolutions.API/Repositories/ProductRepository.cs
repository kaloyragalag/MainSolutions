using MainSolutions.API.Models;
using MainSolutions.API.Data;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
        => await _context.Categories.AnyAsync(c => c.Id == categoryId);

    public override async Task<PagedResult<Product>> GetAllAsync(PaginationQuery query)
    {
        var totalCount = await _dbSet.CountAsync();

        var items = await _dbSet
            .Include(p => p.Category)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
