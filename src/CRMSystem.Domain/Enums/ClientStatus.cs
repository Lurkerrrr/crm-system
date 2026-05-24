namespace CRMSystem.Domain.Enums;

/// <summary>
/// Represents the lifecycle stage of a client relationship.
/// </summary>
public enum ClientStatus
{
    New = 0,
    Active = 1,
    InNegotiation = 2,
    Closed = 3
}