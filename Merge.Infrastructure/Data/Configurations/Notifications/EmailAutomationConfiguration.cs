using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class EmailAutomationConfiguration : IEntityTypeConfiguration<EmailAutomation>
{
    public void Configure(EntityTypeBuilder<EmailAutomation> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsActive);
    }
}
