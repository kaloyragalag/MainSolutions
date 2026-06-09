using MainSolutions.API.Data;
using MainSolutions.API.Models;
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
        var queryable = _dbSet.Include(p => p.Category).AsQueryable();

        queryable = ApplySearch(queryable, query.Search);

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            var sortDesc = query.SortOrder.ToLower() == "desc";
            queryable = query.SortBy.ToLower() switch
            {
                "name"     => sortDesc ? queryable.OrderByDescending(p => p.Name)     : queryable.OrderBy(p => p.Name),
                "price"    => sortDesc ? queryable.OrderByDescending(p => p.Price)    : queryable.OrderBy(p => p.Price),
                "stock"    => sortDesc ? queryable.OrderByDescending(p => p.Stock)    : queryable.OrderBy(p => p.Stock),
                "createdat"=> sortDesc ? queryable.OrderByDescending(p => p.CreatedAt): queryable.OrderBy(p => p.CreatedAt),
                _          => queryable
            };
        }

        var totalCount = await queryable.CountAsync();

        var items = await queryable
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

    protected override IQueryable<Product> ApplySearch(IQueryable<Product> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var term = search.ToLower();
        return query.Where(p =>
            p.Name.ToLower().Contains(term) ||
            p.Description.ToLower().Contains(term) ||
            p.Category.Name.ToLower().Contains(term)
        );
    }
}
