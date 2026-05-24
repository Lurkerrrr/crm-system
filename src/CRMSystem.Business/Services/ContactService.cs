using CRMSystem.Business.Exceptions;
using CRMSystem.Data.Repositories;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Business.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IClientRepository _clientRepository;

    public ContactService(
        IContactRepository contactRepository,
        IClientRepository clientRepository)
    {
        _contactRepository = contactRepository;
        _clientRepository = clientRepository;
    }

    public Task<Contact?> GetByIdAsync(int id) =>
        _contactRepository.GetByIdAsync(id);

    public Task<IEnumerable<Contact>> GetByClientIdAsync(int clientId) =>
        _contactRepository.GetByClientIdAsync(clientId);

    public Task<IEnumerable<Contact>> GetRecentAsync(int count) =>
        _contactRepository.GetRecentAsync(count);

    public async Task<Contact> CreateAsync(Contact contact)
    {
        await ValidateAsync(contact);

        if (contact.Date == default)
            contact.Date = DateTime.UtcNow;

        await _contactRepository.AddAsync(contact);
        await _contactRepository.SaveChangesAsync();

        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        await ValidateAsync(contact);

        var existing = await _contactRepository.GetByIdAsync(contact.Id)
            ?? throw new ValidationException($"Contact with ID {contact.Id} not found.");

        existing.Date = contact.Date;
        existing.Type = contact.Type;
        existing.Description = contact.Description;
        existing.UserId = contact.UserId;

        _contactRepository.Update(existing);
        await _contactRepository.SaveChangesAsync();

        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var contact = await _contactRepository.GetByIdAsync(id)
            ?? throw new ValidationException($"Contact with ID {id} not found.");

        _contactRepository.Remove(contact);
        await _contactRepository.SaveChangesAsync();
    }

    private async Task ValidateAsync(Contact contact)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(contact.Description))
            errors.Add("Description is required.");
        else if (contact.Description.Length > 2000)
            errors.Add("Description cannot exceed 2000 characters.");

        if (contact.Date > DateTime.UtcNow.AddDays(1))
            errors.Add("Contact date cannot be in the future.");

        // Verify the referenced client exists
        if (contact.ClientId <= 0)
        {
            errors.Add("A valid client must be assigned.");
        }
        else
        {
            var client = await _clientRepository.GetByIdAsync(contact.ClientId);
            if (client == null)
                errors.Add($"Client with ID {contact.ClientId} does not exist.");
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}