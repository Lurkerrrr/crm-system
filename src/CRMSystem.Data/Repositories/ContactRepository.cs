using CRMSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Data.Repositories;

public class ContactRepository : Repository<Contact>, IContactRepository
{
    public ContactRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contact>> GetByClientIdAsync(int clientId)
    {
        return await _dbSet
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contact>> GetRecentAsync(int count)
    {
        return await _dbSet
            .Include(c => c.Client)
            .OrderByDescending(c => c.Date)
            .Take(count)
            .ToListAsync();
    }
}