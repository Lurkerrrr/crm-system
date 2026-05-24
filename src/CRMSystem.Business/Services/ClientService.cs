using System.Text.RegularExpressions;
using CRMSystem.Business.Exceptions;
using CRMSystem.Data.Repositories;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.Business.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    // Basic email regex — good enough for a CRM
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public Task<Client?> GetByIdAsync(int id) =>
        _clientRepository.GetByIdAsync(id);

    public Task<Client?> GetByIdWithContactsAsync(int id) =>
        _clientRepository.GetByIdWithContactsAsync(id);

    public Task<IEnumerable<Client>> GetAllAsync() =>
        _clientRepository.GetAllAsync();

    public Task<IEnumerable<Client>> SearchAsync(string searchTerm) =>
        _clientRepository.SearchAsync(searchTerm);

    public Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status) =>
        _clientRepository.GetByStatusAsync(status);

    public async Task<Client> CreateAsync(Client client)
    {
        Validate(client);

        client.CreatedAt = DateTime.UtcNow;
        client.UpdatedAt = null;

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        return client;
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        Validate(client);

        var existing = await _clientRepository.GetByIdAsync(client.Id)
            ?? throw new ValidationException($"Client with ID {client.Id} not found.");

        existing.FirstName = client.FirstName;
        existing.LastName = client.LastName;
        existing.Company = client.Company;
        existing.Email = client.Email;
        existing.Phone = client.Phone;
        existing.Status = client.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        _clientRepository.Update(existing);
        await _clientRepository.SaveChangesAsync();

        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id)
            ?? throw new ValidationException($"Client with ID {id} not found.");

        _clientRepository.Remove(client);
        await _clientRepository.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int clientId, ClientStatus newStatus)
    {
        var client = await _clientRepository.GetByIdAsync(clientId)
            ?? throw new ValidationException($"Client with ID {clientId} not found.");

        // Business rule: cannot reopen a closed client directly — must go back through negotiation
        if (client.Status == ClientStatus.Closed && newStatus == ClientStatus.Active)
        {
            throw new ValidationException(
                "Closed clients must transition through 'In Negotiation' before becoming active again.");
        }

        client.Status = newStatus;
        client.UpdatedAt = DateTime.UtcNow;

        _clientRepository.Update(client);
        await _clientRepository.SaveChangesAsync();
    }

    private static void Validate(Client client)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(client.FirstName))
            errors.Add("First name is required.");
        else if (client.FirstName.Length > 100)
            errors.Add("First name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(client.LastName))
            errors.Add("Last name is required.");
        else if (client.LastName.Length > 100)
            errors.Add("Last name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(client.Email))
            errors.Add("Email is required.");
        else if (!EmailRegex.IsMatch(client.Email))
            errors.Add("Email format is invalid.");
        else if (client.Email.Length > 255)
            errors.Add("Email cannot exceed 255 characters.");

        if (!string.IsNullOrWhiteSpace(client.Company) && client.Company.Length > 200)
            errors.Add("Company name cannot exceed 200 characters.");

        if (!string.IsNullOrWhiteSpace(client.Phone) && client.Phone.Length > 50)
            errors.Add("Phone cannot exceed 50 characters.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}