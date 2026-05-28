using CRMSystem.Business.Services;
using CRMSystem.Data;
using CRMSystem.Data.Repositories;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRMSystem.Tests.Services;

/// <summary>
/// Tests for ReportService using EF Core's InMemory database provider.
/// Unlike the other service tests (which mock repositories), ReportService tests
/// exercise the FULL stack: service → real repository → InMemory database.
/// This catches grouping/counting bugs that mocked tests would miss.
///
/// Each test gets a fresh, isolated database (unique GUID per test).
/// </summary>
public class ReportServiceTests
{
    /// <summary>
    /// Creates a fresh service backed by a unique InMemory database for this test.
    /// Disposing the returned IDisposable is the caller's responsibility (via 'using').
    /// </summary>
    private static (ReportService service, CrmDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CrmDbContext(options);
        var clientRepo = new ClientRepository(context);
        var contactRepo = new ContactRepository(context);
        var service = new ReportService(clientRepo, contactRepo);

        return (service, context);
    }

    // ============================================================
    // TOTAL COUNTS
    // ============================================================

    [Fact]
    public async Task GetTotalClientCountAsync_EmptyDatabase_ReturnsZero()
    {
        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            // Act
            var count = await service.GetTotalClientCountAsync();

            // Assert
            count.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetTotalClientCountAsync_ThreeClients_ReturnsThree()
    {
        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            context.Clients.AddRange(
                new Client { FirstName = "A", LastName = "A", Email = "a@a.com", Status = ClientStatus.New },
                new Client { FirstName = "B", LastName = "B", Email = "b@b.com", Status = ClientStatus.Active },
                new Client { FirstName = "C", LastName = "C", Email = "c@c.com", Status = ClientStatus.Closed }
            );
            await context.SaveChangesAsync();

            // Act
            var count = await service.GetTotalClientCountAsync();

            // Assert
            count.Should().Be(3);
        }
    }

    [Fact]
    public async Task GetTotalContactCountAsync_FiveContacts_ReturnsFive()
    {
        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            var client = new Client { FirstName = "A", LastName = "A", Email = "a@a.com" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                context.Contacts.Add(new Contact
                {
                    ClientId = client.Id,
                    Date = DateTime.UtcNow.AddDays(-i),
                    Type = ContactType.Note,
                    Description = $"Contact {i}"
                });
            }
            await context.SaveChangesAsync();

            // Act
            var count = await service.GetTotalContactCountAsync();

            // Assert
            count.Should().Be(5);
        }
    }

    // ============================================================
    // GROUPING BY STATUS
    // ============================================================

    [Fact]
    public async Task GetClientCountByStatusAsync_MixedStatuses_ReturnsCorrectGrouping()
    {
        // Arrange — 3 New, 2 Active, 1 Closed, 0 InNegotiation
        var (service, context) = CreateService();
        using (context)
        {
            context.Clients.AddRange(
                new Client { FirstName = "A1", LastName = "A1", Email = "a1@a.com", Status = ClientStatus.New },
                new Client { FirstName = "A2", LastName = "A2", Email = "a2@a.com", Status = ClientStatus.New },
                new Client { FirstName = "A3", LastName = "A3", Email = "a3@a.com", Status = ClientStatus.New },
                new Client { FirstName = "B1", LastName = "B1", Email = "b1@b.com", Status = ClientStatus.Active },
                new Client { FirstName = "B2", LastName = "B2", Email = "b2@b.com", Status = ClientStatus.Active },
                new Client { FirstName = "C1", LastName = "C1", Email = "c1@c.com", Status = ClientStatus.Closed }
            );
            await context.SaveChangesAsync();

            // Act
            var grouping = await service.GetClientCountByStatusAsync();

            // Assert
            grouping[ClientStatus.New].Should().Be(3);
            grouping[ClientStatus.Active].Should().Be(2);
            grouping[ClientStatus.Closed].Should().Be(1);
            // InNegotiation has zero clients — it should NOT be in the dictionary
            grouping.Should().NotContainKey(ClientStatus.InNegotiation);
        }
    }

    [Fact]
    public async Task GetClientCountByStatusAsync_EmptyDatabase_ReturnsEmptyDictionary()
    {
        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            // Act
            var grouping = await service.GetClientCountByStatusAsync();

            // Assert
            grouping.Should().BeEmpty();
        }
    }

    // ============================================================
    // GROUPING BY CONTACT TYPE
    // ============================================================

    [Fact]
    public async Task GetContactCountByTypeAsync_MixedTypes_ReturnsCorrectGrouping()
    {
        // Arrange — 2 Meetings, 1 PhoneCall, 3 Emails, 0 Notes
        var (service, context) = CreateService();
        using (context)
        {
            var client = new Client { FirstName = "X", LastName = "X", Email = "x@x.com" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var types = new[]
            {
                ContactType.Meeting, ContactType.Meeting,
                ContactType.PhoneCall,
                ContactType.Email, ContactType.Email, ContactType.Email
            };

            foreach (var type in types)
            {
                context.Contacts.Add(new Contact
                {
                    ClientId = client.Id,
                    Date = DateTime.UtcNow.AddDays(-1),
                    Type = type,
                    Description = $"Test {type}"
                });
            }
            await context.SaveChangesAsync();

            // Act
            var grouping = await service.GetContactCountByTypeAsync();

            // Assert
            grouping[ContactType.Meeting].Should().Be(2);
            grouping[ContactType.PhoneCall].Should().Be(1);
            grouping[ContactType.Email].Should().Be(3);
            grouping.Should().NotContainKey(ContactType.Note);
        }
    }

    // ============================================================
    // DATE-RANGE COUNTING
    // ============================================================

    [Fact]
    public async Task GetContactsInLastDaysAsync_FiltersCorrectly()
    {
        // Arrange — contacts spread across the past 60 days
        var (service, context) = CreateService();
        using (context)
        {
            var client = new Client { FirstName = "X", LastName = "X", Email = "x@x.com" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            context.Contacts.AddRange(
                new Contact { ClientId = client.Id, Date = now.AddDays(-1), Description = "1 day ago", Type = ContactType.Note },
                new Contact { ClientId = client.Id, Date = now.AddDays(-5), Description = "5 days ago", Type = ContactType.Note },
                new Contact { ClientId = client.Id, Date = now.AddDays(-15), Description = "15 days ago", Type = ContactType.Note },
                new Contact { ClientId = client.Id, Date = now.AddDays(-45), Description = "45 days ago", Type = ContactType.Note }
            );
            await context.SaveChangesAsync();

            // Act — last 7 days should match only the first two contacts
            var sevenDays = await service.GetContactsInLastDaysAsync(7);
            var thirtyDays = await service.GetContactsInLastDaysAsync(30);
            var ninetyDays = await service.GetContactsInLastDaysAsync(90);

            // Assert
            sevenDays.Should().Be(2);     // 1 day ago + 5 days ago
            thirtyDays.Should().Be(3);    // + 15 days ago
            ninetyDays.Should().Be(4);    // + 45 days ago = all of them
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetContactsInLastDaysAsync_NonPositiveDays_ReturnsZero(int days)
    {
        // The service short-circuits to 0 when given non-positive days.
        // This guards against meaningless ranges.

        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            var client = new Client { FirstName = "X", LastName = "X", Email = "x@x.com" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            context.Contacts.Add(new Contact
            {
                ClientId = client.Id,
                Date = DateTime.UtcNow,
                Type = ContactType.Note,
                Description = "Should not be counted"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetContactsInLastDaysAsync(days);

            // Assert
            result.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetContactsInLastDaysAsync_BoundaryCase_ExactlyAtThresholdIsCounted()
    {
        // The service uses `c.Date >= threshold`, so a contact dated EXACTLY at the threshold
        // should be included. This test pins that inclusive boundary.

        // Arrange
        var (service, context) = CreateService();
        using (context)
        {
            var client = new Client { FirstName = "X", LastName = "X", Email = "x@x.com" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            // Create a contact dated 5 days ago, then ask for "last 5 days"
            // Add a tiny safety margin so test timing doesn't cause flakes
            context.Contacts.Add(new Contact
            {
                ClientId = client.Id,
                Date = DateTime.UtcNow.AddDays(-5).AddSeconds(1),
                Type = ContactType.Note,
                Description = "Right at the edge"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetContactsInLastDaysAsync(5);

            // Assert — boundary inclusive
            result.Should().Be(1);
        }
    }
}