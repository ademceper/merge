using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
{
    public void Configure(EntityTypeBuilder<FraudAlert> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.PaymentId);
        builder.HasIndex(e => e.AlertType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.Status, e.AlertType });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Reason)
            .HasMaxLength(1000);
        
        builder.Property(e => e.ReviewNotes)
            .HasMaxLength(2000);
        
        builder.Property(e => e.MatchedRules)
            .HasColumnType("jsonb");
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FraudAlert_RiskScore_Range", "\"RiskScore\" >= 0 AND \"RiskScore\" <= 100");
        });
        
        // User relationship (UserId -> User)
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // ReviewedBy relationship (ReviewedByUserId -> ReviewedBy)
        builder.HasOne(e => e.ReviewedBy)
            .WithMany()
            .HasForeignKey(e => e.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Order relationship
        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Payment relationship
        builder.HasOne(e => e.Payment)
            .WithMany()
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
