using CRMSystem.Domain.Entities;

namespace CRMSystem.Data.Repositories;

/// <summary>
/// User-specific repository contract.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}