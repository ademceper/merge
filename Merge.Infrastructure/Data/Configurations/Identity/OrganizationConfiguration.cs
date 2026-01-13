using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

/// <summary>
/// Organization Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.3: Value Objects - Backing field mapping (EF Core compatibility)
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsVerified);
        builder.HasIndex(e => new { e.Status, e.IsVerified });
        
        // ✅ BOLUM 1.3: Value Objects - Backing field mapping for Email
        builder.Property("_email")
            .HasColumnName("Email")
            .HasMaxLength(200);
        
        // ✅ BOLUM 1.3: Value Objects - Backing field mapping for PhoneNumber
        builder.Property("_phone")
            .HasColumnName("Phone")
            .HasMaxLength(50);
        
        // ✅ BOLUM 1.3: Value Objects - Backing field mapping for URL
        builder.Property("_website")
            .HasColumnName("Website")
            .HasMaxLength(500);
        
        // ✅ BOLUM 1.3: Value Objects - Backing field mapping for Address fields
        builder.Property("_addressLine1")
            .HasColumnName("Address")
            .HasMaxLength(500);
        
        builder.Property("_addressLine2")
            .HasColumnName("AddressLine2")
            .HasMaxLength(500);
        
        builder.Property("_city")
            .HasColumnName("City")
            .HasMaxLength(100);
        
        builder.Property("_state")
            .HasColumnName("State")
            .HasMaxLength(100);
        
        builder.Property("_postalCode")
            .HasColumnName("PostalCode")
            .HasMaxLength(20);
        
        builder.Property("_country")
            .HasColumnName("Country")
            .HasMaxLength(100);
        
        // ✅ BOLUM 1.1: Backing field mapping for encapsulated collections
        // ✅ BOLUM 1.4: Aggregate Root Pattern - Navigation property'ler IReadOnlyCollection olduğu için backing field mapping gerekli
        // EF Core backing field'ları otomatik bulur (_fieldName convention: _users, _teams)
        // IReadOnlyCollection property'ler için EF Core otomatik olarak backing field'ı kullanır
        builder.HasMany(e => e.Users)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Teams)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ✅ BOLUM 6.2: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Organization_Name_NotEmpty", "\"Name\" IS NOT NULL AND LENGTH(\"Name\") > 0");
        });
    }
}
