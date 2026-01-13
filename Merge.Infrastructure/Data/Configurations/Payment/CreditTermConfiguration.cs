using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// CreditTerm Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class CreditTermConfiguration : IEntityTypeConfiguration<CreditTerm>
{
    public void Configure(EntityTypeBuilder<CreditTerm> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.OrganizationId, e.IsActive });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.CreditLimit)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.UsedCredit)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.Terms)
            .HasMaxLength(1000);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_CreditTerm_PaymentDays_Positive", "\"PaymentDays\" > 0");
            t.HasCheckConstraint("CK_CreditTerm_CreditLimit_NonNegative", "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0");
            t.HasCheckConstraint("CK_CreditTerm_UsedCredit_NonNegative", "\"UsedCredit\" IS NULL OR \"UsedCredit\" >= 0");
            t.HasCheckConstraint("CK_CreditTerm_UsedCredit_LessThanOrEqual_CreditLimit", "\"CreditLimit\" IS NULL OR \"UsedCredit\" IS NULL OR \"UsedCredit\" <= \"CreditLimit\"");
        });
    }
}
