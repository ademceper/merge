using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.HasOne(e => e.Order)
              .WithMany(e => e.ReturnRequests)
              .HasForeignKey(e => e.OrderId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.User)
              .WithMany(e => e.ReturnRequests)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        
        builder.Property(e => e.RefundAmount).HasPrecision(18, 2);
        builder.Property(e => e.OrderItemIds)
              .HasConversion(
                  v => string.Join(',', v),
                  v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());
                  
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ReturnRequest_RefundAmount_NonNegative", "\"RefundAmount\" >= 0");
        });
    }
}
