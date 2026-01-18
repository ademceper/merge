using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class FraudDetectionRuleConfiguration : IEntityTypeConfiguration<FraudDetectionRule>
{
    public void Configure(EntityTypeBuilder<FraudDetectionRule> builder)
    {
        builder.HasIndex(e => e.RuleType);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => new { e.IsActive, e.Priority });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Conditions)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FraudDetectionRule_RiskScore_Range", "\"RiskScore\" >= 0 AND \"RiskScore\" <= 100");
            t.HasCheckConstraint("CK_FraudDetectionRule_Priority_NonNegative", "\"Priority\" >= 0");
        });
    }
}
