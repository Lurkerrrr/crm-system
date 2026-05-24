using CRMSystem.Domain.Enums;

namespace CRMSystem.Domain.Entities;

/// <summary>
/// Represents an interaction with a client (note, meeting, call, email).
/// </summary>
public class Contact
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public ContactType Type { get; set; } = ContactType.Note;

    public string Description { get; set; } = string.Empty;

    // Foreign key
    public int ClientId { get; set; }

    // Navigation property — each contact belongs to one client
    public Client Client { get; set; } = null!;

    // Optional: link to the user who created this contact
    public int? UserId { get; set; }
    public User? User { get; set; }
}