using CRMSystem.Domain.Enums;

namespace CRMSystem.Business.Services;

public interface IReportService
{
    Task<int> GetTotalClientCountAsync();
    Task<int> GetTotalContactCountAsync();
    Task<Dictionary<ClientStatus, int>> GetClientCountByStatusAsync();
    Task<Dictionary<ContactType, int>> GetContactCountByTypeAsync();
    Task<int> GetContactsInLastDaysAsync(int days);
}