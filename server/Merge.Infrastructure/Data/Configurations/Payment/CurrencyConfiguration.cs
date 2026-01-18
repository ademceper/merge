using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.IsBaseCurrency);
        builder.HasIndex(e => e.IsActive);
        
        builder.HasIndex(e => new { e.IsActive, e.IsBaseCurrency });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Symbol)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.ExchangeRate)
            .HasPrecision(18, 6)
            .IsRequired();
        
        builder.Property(e => e.Format)
            .HasMaxLength(50);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Currency_ExchangeRate_NonNegative", "\"ExchangeRate\" >= 0");
            t.HasCheckConstraint("CK_Currency_DecimalPlaces_Range", "\"DecimalPlaces\" >= 0 AND \"DecimalPlaces\" <= 10");
            t.HasCheckConstraint("CK_Currency_BaseCurrency_Rate", "\"IsBaseCurrency\" = false OR \"ExchangeRate\" = 1.0");
        });
    }
}
