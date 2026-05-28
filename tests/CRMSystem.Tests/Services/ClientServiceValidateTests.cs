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
/// Tests for ClientService validation logic. Validation runs inside CreateAsync
/// and UpdateAsync, so we test it through CreateAsync where validation is the
/// only logic that runs before the repository is touched.
/// </summary>
public class ClientServiceValidateTests
{
    private static ClientService CreateService(out Mock<IClientRepository> repoMock)
    {
        // A fresh mock for every test — no test should affect another.
        repoMock = new Mock<IClientRepository>();

        // We need AddAsync and SaveChangesAsync to be callable without throwing,
        // since CreateAsync calls them after Validate passes.
        repoMock.Setup(r => r.AddAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        return new ClientService(repoMock.Object);
    }

    private static Client ValidClient() => new()
    {
        FirstName = "Anna",
        LastName = "Kowalska",
        Email = "anna@test.pl",
        Status = ClientStatus.New
    };

    // ============================================================
    // FIRST NAME
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_FirstNameMissing_ThrowsValidationError(string? firstName)
    {
        // Arrange
        var service = CreateService(out _);
        var client = ValidClient();
        client.FirstName = firstName!;

        // Act
        var act = async () => await service.CreateAsync(client);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("First name is required.");
    }

    [Fact]
    public async Task CreateAsync_FirstNameTooLong_ThrowsValidationError()
    {
        // Arrange
        var service = CreateService(out _);
        var client = ValidClient();
        client.FirstName = new string('a', 101); // 101 chars — over the 100 limit

        // Act
        var act = async () => await service.CreateAsync(client);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("First name cannot exceed 100 characters.");
    }

    // ============================================================
    // LAST NAME
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_LastNameMissing_ThrowsValidationError(string? lastName)
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.LastName = lastName!;

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Last name is required.");
    }

    [Fact]
    public async Task CreateAsync_LastNameTooLong_ThrowsValidationError()
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.LastName = new string('a', 101);

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Last name cannot exceed 100 characters.");
    }

    // ============================================================
    // EMAIL
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmailMissing_ThrowsValidationError(string? email)
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.Email = email!;

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Email is required.");
    }

    [Theory]
    [InlineData("plainaddress")]       // no @, no dot
    [InlineData("abc@def")]            // no dot
    [InlineData("abc.def")]            // no @
    [InlineData("@nodomain.com")]      // missing local part — should fail; but actually our regex accepts it... let's see
    public async Task CreateAsync_EmailInvalidFormat_ThrowsValidationError(string email)
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.Email = email;

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Email format is invalid.");
    }

    [Theory]
    [InlineData("anna@test.pl")]
    [InlineData("a@b.c")]
    [InlineData("user.name+tag@example.com")]
    public async Task CreateAsync_EmailValidFormat_DoesNotThrow(string email)
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.Email = email;

        var act = async () => await service.CreateAsync(client);

        await act.Should().NotThrowAsync();
    }

    // ============================================================
    // OPTIONAL FIELDS — COMPANY & PHONE
    // ============================================================

    [Fact]
    public async Task CreateAsync_CompanyTooLong_ThrowsValidationError()
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.Company = new string('a', 201);

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Company name cannot exceed 200 characters.");
    }

    [Fact]
    public async Task CreateAsync_PhoneTooLong_ThrowsValidationError()
    {
        var service = CreateService(out _);
        var client = ValidClient();
        client.Phone = new string('1', 51);

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Phone cannot exceed 50 characters.");
    }

    // ============================================================
    // MULTIPLE ERRORS AT ONCE
    // ============================================================

    [Fact]
    public async Task CreateAsync_MultipleErrors_AllReportedTogether()
    {
        // This verifies that Validate accumulates errors instead of throwing on the first one.
        // If someone refactored Validate to short-circuit, this test would catch it.
        var service = CreateService(out _);
        var client = new Client
        {
            FirstName = "",
            LastName = "",
            Email = "not-an-email"
        };

        var act = async () => await service.CreateAsync(client);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        ex.Which.Errors.Should().Contain("First name is required.");
        ex.Which.Errors.Should().Contain("Last name is required.");
        ex.Which.Errors.Should().Contain("Email format is invalid.");
    }

    // ============================================================
    // HAPPY PATH — VALID CLIENT
    // ============================================================

    [Fact]
    public async Task CreateAsync_ValidClient_DoesNotThrowAndCallsRepository()
    {
        // Arrange
        var service = CreateService(out var repoMock);
        var client = ValidClient();

        // Act
        await service.CreateAsync(client);

        // Assert — the mock recorded the calls, we verify they happened
        repoMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}