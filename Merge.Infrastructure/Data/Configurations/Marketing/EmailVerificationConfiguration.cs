using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// EmailVerification EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => e.IsVerified);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.UserId, e.Email, e.IsVerified });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
