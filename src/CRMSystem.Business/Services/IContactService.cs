using CRMSystem.Domain.Entities;

namespace CRMSystem.Business.Services;

public interface IContactService
{
    Task<Contact?> GetByIdAsync(int id);
    Task<IEnumerable<Contact>> GetByClientIdAsync(int clientId);
    Task<IEnumerable<Contact>> GetRecentAsync(int count);
    Task<Contact> CreateAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task DeleteAsync(int id);
}