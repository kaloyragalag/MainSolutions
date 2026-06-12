using MainSolutions.API.Data;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> ExistsAsync(string email)
        => await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    protected override IQueryable<User> ApplySearch(IQueryable<User> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var term = search.ToLower();
        return query.Where(u =>
            u.Username.ToLower().Contains(term) ||
            u.Email.ToLower().Contains(term)
        );
    }
}
