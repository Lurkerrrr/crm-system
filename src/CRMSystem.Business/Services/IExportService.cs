using CRMSystem.Domain.Entities;

namespace CRMSystem.Business.Services;

public interface IExportService
{
    Task ExportClientsToCsvAsync(IEnumerable<Client> clients, string filePath);
    Task ExportClientsToJsonAsync(IEnumerable<Client> clients, string filePath);
}