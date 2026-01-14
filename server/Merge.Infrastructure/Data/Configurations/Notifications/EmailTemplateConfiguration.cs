using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Thumbnail)
            .HasMaxLength(500); // ✅ Domain'deki Guard.AgainstLength ile uyumlu

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsActive);
    }
}
