using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => e.IsVerified);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.UserId, e.Email, e.IsVerified });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
