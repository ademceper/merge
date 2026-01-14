using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// EmailSubscriber EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class EmailSubscriberConfiguration : IEntityTypeConfiguration<EmailSubscriber>
{
    public void Configure(EntityTypeBuilder<EmailSubscriber> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .HasMaxLength(100);

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsSubscribed);
        builder.HasIndex(e => new { e.Email, e.IsSubscribed });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsSent_NonNegative", "\"EmailsSent\" >= 0");
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsOpened_NonNegative", "\"EmailsOpened\" >= 0");
            t.HasCheckConstraint("CK_EmailSubscriber_EmailsClicked_NonNegative", "\"EmailsClicked\" >= 0");
        });
    }
}
