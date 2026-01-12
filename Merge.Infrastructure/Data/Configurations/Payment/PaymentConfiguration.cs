using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

public class PaymentConfiguration : IEntityTypeConfiguration<Merge.Domain.Modules.Payment.Payment>
{
    public void Configure(EntityTypeBuilder<Merge.Domain.Modules.Payment.Payment> builder)
    {
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.Status);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payment_Amount_Positive", "\"Amount\" >= 0");
        });
    }
}
