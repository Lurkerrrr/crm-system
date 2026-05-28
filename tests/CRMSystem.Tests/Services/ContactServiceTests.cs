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
/// Tests for ContactService — validation (including cross-entity "client exists" rule),
/// future-date rule, and CRUD error paths.
/// </summary>
public class ContactServiceTests
{
    /// <summary>
    /// Factory helper. Creates a service with two mocked repositories.
    /// By default, the client repository pretends client #1 exists, so simple tests don't need
    /// to set this up explicitly. Override the setup if a test needs different behavior.
    /// </summary>
    private static ContactService CreateService(
        out Mock<IContactRepository> contactRepo,
        out Mock<IClientRepository> clientRepo)
    {
        contactRepo = new Mock<IContactRepository>();
        clientRepo = new Mock<IClientRepository>();

        contactRepo.Setup(r => r.AddAsync(It.IsAny<Contact>())).Returns(Task.CompletedTask);
        contactRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // By default, ANY client ID lookup returns a real client.
        // Tests that need "client not found" override this for the specific ID they care about.
        clientRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                  .ReturnsAsync((int id) => new Client
                  {
                      Id = id,
                      FirstName = "Anna",
                      LastName = "Kowalska",
                      Email = "anna@test.pl"
                  });

        return new ContactService(contactRepo.Object, clientRepo.Object);
    }

    private static Contact ValidContact() => new()
    {
        ClientId = 1,
        Date = DateTime.UtcNow.AddDays(-1),
        Type = ContactType.Note,
        Description = "Test contact note"
    };

    // ============================================================
    // DESCRIPTION VALIDATION
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_DescriptionMissing_ThrowsValidationError(string? description)
    {
        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.Description = description!;

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Description is required.");
    }

    [Fact]
    public async Task CreateAsync_DescriptionTooLong_ThrowsValidationError()
    {
        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.Description = new string('a', 2001); // 2001 chars — over the 2000 limit

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Description cannot exceed 2000 characters.");
    }

    // ============================================================
    // FUTURE-DATE RULE
    // ============================================================

    [Fact]
    public async Task CreateAsync_DateMoreThanOneDayInFuture_ThrowsValidationError()
    {
        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.Date = DateTime.UtcNow.AddDays(2); // 2 days in the future — over the 1-day grace period

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Contact date cannot be in the future.");
    }

    [Fact]
    public async Task CreateAsync_DateWithinOneDayGrace_IsAllowed()
    {
        // The rule allows dates up to 24 hours in the future (timezone safety).
        // This test pins that behavior — if someone tightens the rule to "strictly past or today",
        // this test would fail and force the discussion.

        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.Date = DateTime.UtcNow.AddHours(12); // 12 hours in the future — within grace

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_PastDate_IsAllowed()
    {
        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.Date = DateTime.UtcNow.AddDays(-30);

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ============================================================
    // CROSS-ENTITY: CLIENT MUST EXIST
    // ============================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task CreateAsync_InvalidClientId_ThrowsValidationError(int clientId)
    {
        // Arrange
        var service = CreateService(out _, out _);
        var contact = ValidContact();
        contact.ClientId = clientId;

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("A valid client must be assigned.");
    }

    [Fact]
    public async Task CreateAsync_ClientDoesNotExist_ThrowsValidationError()
    {
        // The MOST IMPORTANT cross-entity test: a contact cannot reference a non-existent client.
        // This is referential integrity enforced at the application layer.

        // Arrange
        var service = CreateService(out _, out var clientRepo);
        var contact = ValidContact();
        contact.ClientId = 999;

        // Override the default mock setup to return NULL for clientId 999
        clientRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Client?)null);

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999") && e.Contains("does not exist"));
    }

    // ============================================================
    // MULTIPLE ERRORS AT ONCE
    // ============================================================

    [Fact]
    public async Task CreateAsync_MultipleErrors_AllReportedTogether()
    {
        // Arrange — invalid description AND future date AND invalid client ID
        var service = CreateService(out _, out _);
        var contact = new Contact
        {
            ClientId = 0,
            Date = DateTime.UtcNow.AddDays(5),
            Type = ContactType.Note,
            Description = ""
        };

        // Act
        var act = async () => await service.CreateAsync(contact);

        // Assert — at least 3 errors reported
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        ex.Which.Errors.Should().Contain("Description is required.");
        ex.Which.Errors.Should().Contain("Contact date cannot be in the future.");
        ex.Which.Errors.Should().Contain("A valid client must be assigned.");
    }

    // ============================================================
    // UPDATE — NOT FOUND HANDLING
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ContactNotFound_ThrowsValidationException()
    {
        // Arrange — valid input, but the contact ID doesn't exist
        var service = CreateService(out var contactRepo, out _);
        var contact = ValidContact();
        contact.Id = 999;
        // contactRepo.GetByIdAsync(999) returns null by default

        // Act
        var act = async () => await service.UpdateAsync(contact);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999") && e.Contains("Contact"));
    }

    [Fact]
    public async Task UpdateAsync_ValidExistingContact_UpdatesFields()
    {
        // Arrange
        var service = CreateService(out var contactRepo, out _);
        var existing = new Contact
        {
            Id = 5,
            ClientId = 1,
            Date = DateTime.UtcNow.AddDays(-10),
            Type = ContactType.Note,
            Description = "Old description"
        };
        contactRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);

        var update = new Contact
        {
            Id = 5,
            ClientId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            Type = ContactType.Meeting,
            Description = "New meeting note"
        };

        // Act
        var result = await service.UpdateAsync(update);

        // Assert
        result.Type.Should().Be(ContactType.Meeting);
        result.Description.Should().Be("New meeting note");
    }

    // ============================================================
    // DELETE — NOT FOUND HANDLING
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ContactNotFound_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService(out _, out _);
        // contactRepo.GetByIdAsync(999) returns null by default

        // Act
        var act = async () => await service.DeleteAsync(999);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("999"));
    }

    [Fact]
    public async Task DeleteAsync_ContactExists_CallsRepositoryRemoveAndSave()
    {
        // Arrange
        var service = CreateService(out var contactRepo, out _);
        var contact = new Contact { Id = 5, ClientId = 1, Description = "x" };
        contactRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(contact);

        // Act
        await service.DeleteAsync(5);

        // Assert
        contactRepo.Verify(r => r.Remove(contact), Times.Once);
        contactRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ============================================================
    // HAPPY PATH
    // ============================================================

    [Fact]
    public async Task CreateAsync_ValidContact_CallsRepositoryAndReturnsContact()
    {
        // Arrange
        var service = CreateService(out var contactRepo, out _);
        var contact = ValidContact();

        // Act
        var result = await service.CreateAsync(contact);

        // Assert
        result.Should().NotBeNull();
        contactRepo.Verify(r => r.AddAsync(It.IsAny<Contact>()), Times.Once);
        contactRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}