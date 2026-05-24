using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.Business.Services;

public interface IClientService
{
    Task<Client?> GetByIdAsync(int id);
    Task<Client?> GetByIdWithContactsAsync(int id);
    Task<IEnumerable<Client>> GetAllAsync();
    Task<IEnumerable<Client>> SearchAsync(string searchTerm);
    Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status);
    Task<Client> CreateAsync(Client client);
    Task<Client> UpdateAsync(Client client);
    Task DeleteAsync(int id);
    Task ChangeStatusAsync(int clientId, ClientStatus newStatus);
}