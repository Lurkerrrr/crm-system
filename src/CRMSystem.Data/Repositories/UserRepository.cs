using CRMSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }
}