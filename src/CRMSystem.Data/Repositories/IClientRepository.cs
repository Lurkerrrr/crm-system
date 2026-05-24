using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.Data.Repositories;

/// <summary>
/// Client-specific repository contract.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByIdWithContactsAsync(int id);
    Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status);
    Task<IEnumerable<Client>> SearchAsync(string searchTerm);
}