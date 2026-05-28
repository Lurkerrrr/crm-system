using System.Text;
using System.Text.Json;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CRMSystem.Tests.Services;

/// <summary>
/// Tests for ExportService. Unlike other service tests, these write to real files
/// (in the system temp folder) and read them back to verify format correctness.
/// Each test cleans up its temp file in a finally block.
/// </summary>
public class ExportServiceTests
{
    private static List<Client> SampleClients() => new()
    {
        new Client
        {
            Id = 1,
            FirstName = "Anna",
            LastName = "Kowalska",
            Company = "TechSoft",
            Email = "anna@techsoft.pl",
            Phone = "+48 600 100 200",
            Status = ClientStatus.Active,
            CreatedAt = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        },
        new Client
        {
            Id = 2,
            FirstName = "Maria",
            LastName = "Wiśniewska",
            Company = "GreenLeaf",
            Email = "m.wisniewska@greenleaf.pl",
            Phone = null,
            Status = ClientStatus.InNegotiation,
            CreatedAt = new DateTime(2026, 1, 16, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        },
        new Client
        {
            Id = 3,
            FirstName = "Piotr",
            LastName = "Zieliński",
            Company = null,
            Email = "piotr.z@gmail.com",
            Phone = null,
            Status = ClientStatus.New,
            CreatedAt = new DateTime(2026, 1, 17, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        }
    };

    // ============================================================
    // CSV TESTS
    // ============================================================

    [Fact]
    public async Task ExportClientsToCsvAsync_WritesHeaderRow_WithExpectedColumns()
    {
        // The CSV header must include all the columns the user expects to see.
        // A missing column would silently drop data; an extra column would surprise users.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToCsvAsync(clients, path);

            // Assert
            var lines = await File.ReadAllLinesAsync(path);
            lines.Should().NotBeEmpty();

            var header = lines[0];
            header.Should().Contain("Id");
            header.Should().Contain("FirstName");
            header.Should().Contain("LastName");
            header.Should().Contain("Company");
            header.Should().Contain("Email");
            header.Should().Contain("Phone");
            header.Should().Contain("Status");
            header.Should().Contain("CreatedAt");
            header.Should().Contain("UpdatedAt");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToCsvAsync_WritesOneRowPerClient()
    {
        // 3 clients → 4 lines (1 header + 3 data rows).
        // Catches "off-by-one" or "first row swallowed" kinds of bugs.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToCsvAsync(clients, path);

            // Assert
            var lines = await File.ReadAllLinesAsync(path);
            lines.Should().HaveCount(4); // 1 header + 3 data rows
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToCsvAsync_PreservesPolishCharacters_AsUtf8()
    {
        // Phase 12 fix: CSV must be written as UTF-8 with BOM so Polish characters
        // (Wiśniewska, Zieliński) round-trip cleanly.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToCsvAsync(clients, path);

            // Assert — read with explicit UTF-8 to confirm encoding
            var contents = await File.ReadAllTextAsync(path, Encoding.UTF8);
            contents.Should().Contain("Wiśniewska");
            contents.Should().Contain("Zieliński");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToCsvAsync_WritesUtf8BomMarker()
    {
        // Phase 12 fix: the file must start with the UTF-8 BOM bytes 0xEF, 0xBB, 0xBF.
        // Excel uses this marker to detect UTF-8 encoding (without it, Excel defaults
        // to Windows-1250 and mangles Polish characters).

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToCsvAsync(clients, path);

            // Assert
            var rawBytes = await File.ReadAllBytesAsync(path);
            rawBytes.Should().HaveCountGreaterThan(3); // sanity check
            rawBytes[0].Should().Be(0xEF);
            rawBytes[1].Should().Be(0xBB);
            rawBytes[2].Should().Be(0xBF);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToCsvAsync_EmptyList_WritesHeaderOnly()
    {
        // Edge case: exporting zero clients should still produce a valid CSV
        // (header row only, no data rows). Catches null-reference bugs in the export pipeline.

        // Arrange
        var service = new ExportService();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToCsvAsync(new List<Client>(), path);

            // Assert
            var lines = await File.ReadAllLinesAsync(path);
            lines.Should().HaveCount(1); // header only
            lines[0].Should().Contain("Id"); // header still present
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    // ============================================================
    // JSON TESTS
    // ============================================================

    [Fact]
    public async Task ExportClientsToJsonAsync_ProducesValidJson()
    {
        // Output must be parseable as JSON. If we ever introduce a serialization bug,
        // this test fails immediately on the JsonDocument.Parse call.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToJsonAsync(clients, path);

            // Assert
            var json = await File.ReadAllTextAsync(path);
            var act = () => JsonDocument.Parse(json);
            act.Should().NotThrow();

            // Verify it's an array with the right count
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
            doc.RootElement.GetArrayLength().Should().Be(3);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToJsonAsync_UsesCamelCasePropertyNames()
    {
        // Phase 12 decision: JSON uses camelCase (firstName, not FirstName) — standard convention.
        // Catches accidental switch back to PascalCase from default serializer settings.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToJsonAsync(clients, path);

            // Assert
            var json = await File.ReadAllTextAsync(path);
            json.Should().Contain("\"firstName\":");
            json.Should().Contain("\"lastName\":");
            json.Should().Contain("\"email\":");
            json.Should().NotContain("\"FirstName\":");  // PascalCase should NOT appear
            json.Should().NotContain("\"LastName\":");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToJsonAsync_PreservesPolishCharacters_NotEscaped()
    {
        // Phase 12 fix: the Unicode-range Encoder lets Polish characters appear as-is
        // rather than as \u015B sequences. We verify both the positive ("Wiśniewska" appears)
        // and the negative (the escape sequence does NOT appear).

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToJsonAsync(clients, path);

            // Assert
            var json = await File.ReadAllTextAsync(path);

            // Positive: real Polish characters appear in the output
            json.Should().Contain("Wiśniewska");
            json.Should().Contain("Zieliński");

            // Negative: the Unicode escape sequences do NOT appear
            json.Should().NotContain("\\u015B");  // ś
            json.Should().NotContain("\\u0144");  // ń
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToJsonAsync_OmitsContactsNavigationProperty()
    {
        // Phase 12 fix: the flat DTO projection excludes the Contacts navigation property.
        // Including it would either serialize the whole graph (huge files) or cause cycles.

        // Arrange
        var service = new ExportService();
        var clients = SampleClients();
        // Even if Contacts is populated, it should NOT appear in the output
        clients[0].Contacts = new List<Contact>
        {
            new() { Id = 100, ClientId = 1, Description = "Should not appear in export" }
        };
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToJsonAsync(clients, path);

            // Assert
            var json = await File.ReadAllTextAsync(path);
            json.Should().NotContain("contacts");      // no camelCase key
            json.Should().NotContain("Contacts");      // no PascalCase key either
            json.Should().NotContain("Should not appear in export");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportClientsToJsonAsync_EmptyList_WritesEmptyArray()
    {
        // Edge case: zero clients → "[]" — must still be valid JSON.

        // Arrange
        var service = new ExportService();
        var path = Path.GetTempFileName();

        try
        {
            // Act
            await service.ExportClientsToJsonAsync(new List<Client>(), path);

            // Assert
            var json = await File.ReadAllTextAsync(path);
            json.Trim().Should().Be("[]");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}