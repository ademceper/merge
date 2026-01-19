using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;


public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsVerified);
        builder.HasIndex(e => new { e.Status, e.IsVerified });
        
        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .HasMaxLength(200);
        
        builder.Property(e => e.Phone)
            .HasColumnName("Phone")
            .HasMaxLength(50);
        
        builder.Property(e => e.Website)
            .HasColumnName("Website")
            .HasMaxLength(500);
        
        builder.Property(e => e.Address)
            .HasColumnName("Address")
            .HasMaxLength(500);
        
        builder.Property(e => e.AddressLine2)
            .HasColumnName("AddressLine2")
            .HasMaxLength(500);
        
        builder.Property(e => e.City)
            .HasColumnName("City")
            .HasMaxLength(100);
        
        builder.Property(e => e.State)
            .HasColumnName("State")
            .HasMaxLength(100);
        
        builder.Property(e => e.PostalCode)
            .HasColumnName("PostalCode")
            .HasMaxLength(20);
        
        builder.Property(e => e.Country)
            .HasColumnName("Country")
            .HasMaxLength(100);
        
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
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Organization_Name_NotEmpty", "\"Name\" IS NOT NULL AND LENGTH(\"Name\") > 0");
        });
    }
}
