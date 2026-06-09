using System.Linq.Expressions;
using MainSolutions.API.Data;
using MainSolutions.API.Models.DTOs;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<PagedResult<T>> GetAllAsync(PaginationQuery query)
    {
        var queryable = _dbSet.AsQueryable();

        queryable = ApplySearch(queryable, query.Search);
        queryable = ApplySort(queryable, query.SortBy, query.SortOrder);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<T> CreateAsync(T entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
        => await _dbSet.FindAsync(id) is not null;

    // Override in derived repositories to define searchable fields
    protected virtual IQueryable<T> ApplySearch(IQueryable<T> query, string? search)
        => query;

    private static IQueryable<T> ApplySort(IQueryable<T> query, string? sortBy, string sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return query;

        var property = typeof(T).GetProperty(
            sortBy,
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance
        );

        if (property is null) return query;

        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.PropertyOrField(param, property.Name);
        var lambda = Expression.Lambda(body, param);

        var methodName = sortOrder.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
    }
}
