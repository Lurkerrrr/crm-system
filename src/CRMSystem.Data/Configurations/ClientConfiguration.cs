using CRMSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMSystem.Data.Configurations;

/// <summary>
/// Fluent API configuration for the Client entity.
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Company)
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>(); // store enum as int in DB

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Index on Email for faster lookups
        builder.HasIndex(c => c.Email);

        // Ignore the computed property
        builder.Ignore(c => c.FullName);

        // Relationship: Client has many Contacts
        builder.HasMany(c => c.Contacts)
            .WithOne(ct => ct.Client)
            .HasForeignKey(ct => ct.ClientId)
            .OnDelete(DeleteBehavior.Cascade); // deleting client deletes their contacts
    }
}