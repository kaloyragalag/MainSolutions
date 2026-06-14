using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
    }

    protected override IQueryable<Customer> ApplySearch(IQueryable<Customer> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var term = search.ToLower();
        return query.Where(c =>
            c.FirstName.ToLower().Contains(term) ||
            c.LastName.ToLower().Contains(term) ||
            c.City!.ToLower().Contains(term) ||
            c.Country!.ToLower().Contains(term) ||
            c.Province!.ToLower().Contains(term) ||
            c.PostalCode!.ToLower().Contains(term) ||
            c.Phone!.ToLower().Contains(term)
        );
    }
}
