using CRMSystem.Domain.Enums;

namespace CRMSystem.Domain.Entities;

/// <summary>
/// Represents a user of the CRM system (for optional authentication).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // Stores the BCrypt hash, NEVER the plain password
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — a user can create many contacts
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}