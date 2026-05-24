using CRMSystem.Domain.Enums;

namespace CRMSystem.Domain.Entities;

/// <summary>
/// Represents a customer/client in the CRM system.
/// </summary>
public class Client
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property — one client has many contacts
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    // Convenience property (not mapped to DB)
    public string FullName => $"{FirstName} {LastName}".Trim();
}