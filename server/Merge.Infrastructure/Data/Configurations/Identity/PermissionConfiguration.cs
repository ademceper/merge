using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Resource);
        builder.HasIndex(e => new { e.Resource, e.Action });
        builder.HasIndex(e => e.IsSystemPermission);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Resource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasMany(e => e.RolePermissions)
            .WithOne(e => e.Permission)
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Permission_Name_NotEmpty", "\"Name\" IS NOT NULL AND LENGTH(\"Name\") > 0");
            t.HasCheckConstraint("CK_Permission_Category_NotEmpty", "\"Category\" IS NOT NULL AND LENGTH(\"Category\") > 0");
            t.HasCheckConstraint("CK_Permission_Resource_NotEmpty", "\"Resource\" IS NOT NULL AND LENGTH(\"Resource\") > 0");
            t.HasCheckConstraint("CK_Permission_Action_NotEmpty", "\"Action\" IS NOT NULL AND LENGTH(\"Action\") > 0");
        });
    }
}
