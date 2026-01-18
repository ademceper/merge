using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Analytics;

namespace Merge.Infrastructure.Data.Configurations.Analytics;


public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
    {
        builder.HasIndex(e => e.CurrencyId);
        builder.HasIndex(e => e.CurrencyCode);
        builder.HasIndex(e => e.RecordedAt);
        builder.HasIndex(e => new { e.CurrencyId, e.RecordedAt });
        builder.HasIndex(e => new { e.CurrencyCode, e.RecordedAt });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.ExchangeRate)
            .HasPrecision(18, 6)
            .IsRequired();
        
        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(50);
        
        // Navigation properties
        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ExchangeRateHistory_ExchangeRate_NonNegative", "\"ExchangeRate\" >= 0");
            t.HasCheckConstraint("CK_ExchangeRateHistory_CurrencyCode_Length", "LENGTH(\"CurrencyCode\") >= 3 AND LENGTH(\"CurrencyCode\") <= 10");
        });
    }
}
