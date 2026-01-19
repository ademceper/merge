using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(e => e.RoleType);
        builder.HasIndex(e => e.IsSystemRole);
        builder.HasIndex(e => new { e.RoleType, e.IsSystemRole });

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RoleType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany<RolePermission>()
            .WithOne(e => e.Role)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
