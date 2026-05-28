using CRMSystem.Business.Exceptions;
using CRMSystem.Business.Services;
using CRMSystem.Data.Repositories;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRMSystem.Tests.Services;

/// <summary>
/// Tests for ClientService business logic — state transitions,
/// not-found handling, and timestamp management.
/// </summary>
public class ClientServiceBusinessLogicTests
{
    private static ClientService CreateService(out Mock<IClientRepository> repoMock)
    {
        repoMock = new Mock<IClientRepository>();
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        return new ClientService(repoMock.Object);
    }

    private static Client ExistingClient(int id, ClientStatus status) => new()
    {
        Id = id,
        FirstName = "Anna",
        LastName = "Kowalska",
        Email = "anna@test.pl",
        Status = status,
        CreatedAt = DateTime.UtcNow.AddDays(-30)
    };

    // ============================================================
    // CHANGE STATUS — THE STATE MACHINE RULE
    // ============================================================

    [Fact]
    public async Task ChangeStatusAsync_ClosedToActive_ThrowsValidationException()
    {
        // The flagship business rule: a closed client cannot be reactivated directly.
        // They must go through "InNegotiation" first.

        // Arrange
        var service = CreateService(out var repoMock);
        var closedClient = ExistingClient(1, ClientStatus.Closed);
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(closedClient);

        // Act
        var act = async () => await service.ChangeStatusAsync(1, ClientStatus.Active);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Closed clients must transition"));
    }

    [Theory]
    [InlineData(ClientStatus.New)]
    [InlineData(ClientStatus.InNegotiation)]
    [InlineData(ClientStatus.Closed)]
    public async Task ChangeStatusAsync_ClosedToOtherStatus_IsAllowed(ClientStatus targetStatus)
    {
        // The rule only blocks Closed → Active. Other transitions FROM Closed are fine
        // (e.g., reopening through InNegotiation, or keeping it Closed).

        // Arrange
        var service = CreateService(out var repoMock);
        var closedClient = ExistingClient(1, ClientStatus.Closed);
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(closedClient);

        // Act
        var act = async () => await service.ChangeStatusAsync(1, targetStatus);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(ClientStatus.New, ClientStatus.Active)]
    [InlineData(ClientStatus.New, ClientStatus.InNegotiation)]
    [InlineData(ClientStatus.Active, ClientStatus.InNegotiation)]
    [InlineData(ClientStatus.InNegotiation, ClientStatus.Active)]
    [InlineData(ClientStatus.Active, ClientStatus.Closed)]
    public async Task ChangeStatusAsync_OtherTransitions_AreAllowed(
        ClientStatus from, ClientStatus to)
    {
        // Sanity check: standard transitions should all work.

        // Arrange
        var service = CreateService(out var repoMock);
        var client = ExistingClient(1, from);
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        var act = async () => await service.ChangeStatusAsync(1, to);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ChangeStatusAsync_ClientNotFound_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService(out var repoMock);
        // Default mock behavior: GetByIdAsync(999) returns null — no setup needed

        // Act
        var act = async () => await service.ChangeStatusAsync(999, ClientStatus.Active);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999") && e.Contains("not found"));
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_UpdatesTimestamp()
    {
        // This verifies that UpdatedAt is set when status changes — easy to forget
        // in a refactor, and important for audit trails.

        // Arrange
        var service = CreateService(out var repoMock);
        var client = ExistingClient(1, ClientStatus.New);
        var originalUpdatedAt = client.UpdatedAt;
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        await service.ChangeStatusAsync(1, ClientStatus.Active);

        // Assert
        client.UpdatedAt.Should().NotBe(originalUpdatedAt);
        client.UpdatedAt.Should().NotBeNull();
        client.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_CallsRepositoryUpdateAndSave()
    {
        // Arrange
        var service = CreateService(out var repoMock);
        var client = ExistingClient(1, ClientStatus.New);
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        await service.ChangeStatusAsync(1, ClientStatus.Active);

        // Assert — the right repository methods were called
        repoMock.Verify(r => r.Update(It.IsAny<Client>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ============================================================
    // UPDATE — NOT FOUND HANDLING
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ClientNotFound_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService(out _);
        var client = new Client
        {
            Id = 999,
            FirstName = "Anna",
            LastName = "Kowalska",
            Email = "anna@test.pl"
        };

        // Act
        var act = async () => await service.UpdateAsync(client);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999") && e.Contains("not found"));
    }

    [Fact]
    public async Task UpdateAsync_ValidExistingClient_UpdatesFieldsAndSetsTimestamp()
    {
        // Arrange
        var service = CreateService(out var repoMock);
        var existing = ExistingClient(1, ClientStatus.New);
        existing.FirstName = "OldName";
        existing.UpdatedAt = null;
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var update = new Client
        {
            Id = 1,
            FirstName = "NewName",
            LastName = "Updated",
            Email = "new@test.pl",
            Status = ClientStatus.Active
        };

        // Act
        var result = await service.UpdateAsync(update);

        // Assert
        result.FirstName.Should().Be("NewName");
        result.LastName.Should().Be("Updated");
        result.Email.Should().Be("new@test.pl");
        result.Status.Should().Be(ClientStatus.Active);
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ============================================================
    // DELETE — NOT FOUND HANDLING
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ClientNotFound_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService(out _);

        // Act
        var act = async () => await service.DeleteAsync(999);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999") && e.Contains("not found"));
    }

    [Fact]
    public async Task DeleteAsync_ClientExists_CallsRepositoryRemoveAndSave()
    {
        // Arrange
        var service = CreateService(out var repoMock);
        var client = ExistingClient(1, ClientStatus.New);
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        await service.DeleteAsync(1);

        // Assert
        repoMock.Verify(r => r.Remove(client), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ============================================================
    // CREATE — TIMESTAMP MANAGEMENT
    // ============================================================

    [Fact]
    public async Task CreateAsync_SetsCreatedAtAndClearsUpdatedAt()
    {
        // CreatedAt should be set to now; UpdatedAt should be null on creation.

        // Arrange
        var service = CreateService(out _);
        var client = new Client
        {
            FirstName = "Anna",
            LastName = "Kowalska",
            Email = "anna@test.pl",
            Status = ClientStatus.New,
            UpdatedAt = DateTime.UtcNow.AddDays(-10) // even if caller sets it, we should clear it
        };

        // Act
        var result = await service.CreateAsync(client);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeNull();
    }
}