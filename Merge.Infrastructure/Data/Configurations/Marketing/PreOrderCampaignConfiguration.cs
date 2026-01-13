using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// PreOrderCampaign EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class PreOrderCampaignConfiguration : IEntityTypeConfiguration<PreOrderCampaign>
{
    public void Configure(EntityTypeBuilder<PreOrderCampaign> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.DepositPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.SpecialPrice)
            .HasPrecision(18, 2);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
        builder.HasIndex(e => new { e.ProductId, e.IsActive });
        builder.HasIndex(e => new { e.IsActive, e.StartDate, e.EndDate });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_PreOrderCampaign_MaxQuantity_NonNegative", "\"MaxQuantity\" >= 0");
            t.HasCheckConstraint("CK_PreOrderCampaign_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0");
            t.HasCheckConstraint("CK_PreOrderCampaign_CurrentQuantity_LessThan_MaxQuantity", "\"MaxQuantity\" = 0 OR \"CurrentQuantity\" <= \"MaxQuantity\"");
            t.HasCheckConstraint("CK_PreOrderCampaign_DepositPercentage_Range", "\"DepositPercentage\" >= 0 AND \"DepositPercentage\" <= 100");
            t.HasCheckConstraint("CK_PreOrderCampaign_SpecialPrice_NonNegative", "\"SpecialPrice\" >= 0");
            t.HasCheckConstraint("CK_PreOrderCampaign_EndDate_After_StartDate", "\"EndDate\" > \"StartDate\"");
            t.HasCheckConstraint("CK_PreOrderCampaign_ExpectedDeliveryDate_After_EndDate", "\"ExpectedDeliveryDate\" > \"EndDate\"");
        });
    }
}
