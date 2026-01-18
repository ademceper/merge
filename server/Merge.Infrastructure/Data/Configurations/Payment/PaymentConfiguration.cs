using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.OrderId, e.Status });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.PaymentMethod)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.PaymentProvider)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.TransactionId)
            .HasMaxLength(255);
        
        builder.Property(e => e.PaymentReference)
            .HasMaxLength(255);
        
        builder.Property(e => e.FailureReason)
            .HasMaxLength(1000);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payment_Amount_Positive", "\"Amount\" > 0");
        });
    }
}
