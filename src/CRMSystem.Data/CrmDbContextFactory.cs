using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRMSystem.Data;

/// <summary>
/// Design-time factory used by EF Core CLI tools (migrations).
/// NOT used at runtime — the WPF app configures DbContext via DI.
/// </summary>
public class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        optionsBuilder.UseSqlite("Data Source=crm.db");

        return new CrmDbContext(optionsBuilder.Options);
    }
}