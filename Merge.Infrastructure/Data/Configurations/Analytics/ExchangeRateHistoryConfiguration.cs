using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Analytics;

namespace Merge.Infrastructure.Data.Configurations.Analytics;

public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
    {
        builder.Property(e => e.ExchangeRate).HasPrecision(18, 6);
        builder.HasIndex(e => new { e.CurrencyId, e.RecordedAt });
    }
}
