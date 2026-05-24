using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Data.Repositories;

/// <summary>
/// Client-specific repository with eager-loading and search support.
/// </summary>
public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(CrmDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a client with their full contact history eagerly loaded.
    /// </summary>
    public async Task<Client?> GetByIdWithContactsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    /// <summary>
    /// Case-insensitive search across name, company, and email fields.
    /// </summary>
    public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();

        return await _dbSet
            .Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.Company != null && c.Company.ToLower().Contains(term)) ||
                c.Email.ToLower().Contains(term))
            .ToListAsync();
    }
}