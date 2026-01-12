using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
{
    public void Configure(EntityTypeBuilder<EmailCampaign> builder)
    {
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(255);
    }
}
