using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class EmailSubscriberConfiguration : IEntityTypeConfiguration<EmailSubscriber>
{
    public void Configure(EntityTypeBuilder<EmailSubscriber> builder)
    {
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .HasMaxLength(100);

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsSubscribed);
        builder.HasIndex(e => new { e.Email, e.IsSubscribed });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsSent_NonNegative", "\"EmailsSent\" >= 0");
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsOpened_NonNegative", "\"EmailsOpened\" >= 0");
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsClicked_NonNegative", "\"EmailsClicked\" >= 0");
        });
    }
}
