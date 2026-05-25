using System.Globalization;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using CsvHelper;
using CsvHelper.Configuration;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Business.Services;

public class ExportService : IExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
    };

    public async Task ExportClientsToCsvAsync(IEnumerable<Client> clients, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        // Project to a flat DTO so we don't dump navigation properties (Contacts) into CSV.
        var rows = clients.Select(c => new ClientCsvRow
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Company = c.Company,
            Email = c.Email,
            Phone = c.Phone,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        await using var writer = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(rows);
    }

    public async Task ExportClientsToJsonAsync(IEnumerable<Client> clients, string filePath)
    {
        // Project to a flat DTO so navigation properties don't pollute the JSON.
        var rows = clients.Select(c => new
        {
            c.Id,
            c.FirstName,
            c.LastName,
            c.Company,
            c.Email,
            c.Phone,
            Status = c.Status.ToString(),
            c.CreatedAt,
            c.UpdatedAt
        });

        var json = JsonSerializer.Serialize(rows, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    private class ClientCsvRow
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Company { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}