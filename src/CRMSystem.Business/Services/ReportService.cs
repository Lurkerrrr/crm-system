using CRMSystem.Data.Repositories;
using CRMSystem.Domain.Enums;

namespace CRMSystem.Business.Services;

public class ReportService : IReportService
{
    private readonly IClientRepository _clientRepository;
    private readonly IContactRepository _contactRepository;

    public ReportService(
        IClientRepository clientRepository,
        IContactRepository contactRepository)
    {
        _clientRepository = clientRepository;
        _contactRepository = contactRepository;
    }

    public async Task<int> GetTotalClientCountAsync()
    {
        var all = await _clientRepository.GetAllAsync();
        return all.Count();
    }

    public async Task<int> GetTotalContactCountAsync()
    {
        var all = await _contactRepository.GetAllAsync();
        return all.Count();
    }

    public async Task<Dictionary<ClientStatus, int>> GetClientCountByStatusAsync()
    {
        var all = await _clientRepository.GetAllAsync();
        return all
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<ContactType, int>> GetContactCountByTypeAsync()
    {
        var all = await _contactRepository.GetAllAsync();
        return all
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<int> GetContactsInLastDaysAsync(int days)
    {
        if (days <= 0) return 0;

        var threshold = DateTime.UtcNow.AddDays(-days);
        var all = await _contactRepository.GetAllAsync();
        return all.Count(c => c.Date >= threshold);
    }
}