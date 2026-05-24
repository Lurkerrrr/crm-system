using CRMSystem.Domain.Entities;

namespace CRMSystem.Data.Repositories;

/// <summary>
/// Contact-specific repository contract.
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    Task<IEnumerable<Contact>> GetByClientIdAsync(int clientId);
    Task<IEnumerable<Contact>> GetRecentAsync(int count);
}