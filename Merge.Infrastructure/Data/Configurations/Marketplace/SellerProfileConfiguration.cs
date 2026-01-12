using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Infrastructure.Data.Configurations.Marketplace;

public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.Status);
        
        builder.Property(e => e.CommissionRate).HasPrecision(5, 2);
        builder.Property(e => e.TotalEarnings).HasPrecision(18, 2);
        builder.Property(e => e.PendingBalance).HasPrecision(18, 2);
        builder.Property(e => e.AvailableBalance).HasPrecision(18, 2);
        builder.Property(e => e.AverageRating).HasPrecision(3, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SellerProfile_CommissionRate_Range", "\"CommissionRate\" >= 0 AND \"CommissionRate\" <= 100");
            t.HasCheckConstraint("CK_SellerProfile_TotalEarnings_NonNegative", "\"TotalEarnings\" >= 0");
            t.HasCheckConstraint("CK_SellerProfile_PendingBalance_NonNegative", "\"PendingBalance\" >= 0");
            t.HasCheckConstraint("CK_SellerProfile_AvailableBalance_NonNegative", "\"AvailableBalance\" >= 0");
            t.HasCheckConstraint("CK_SellerProfile_AverageRating_Range", "\"AverageRating\" >= 0 AND \"AverageRating\" <= 5");
        });
    }
}
