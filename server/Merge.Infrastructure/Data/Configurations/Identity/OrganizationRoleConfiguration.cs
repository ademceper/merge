using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class OrganizationRoleConfiguration : IEntityTypeConfiguration<OrganizationRole>
{
    public void Configure(EntityTypeBuilder<OrganizationRole> builder)
    {
        builder.HasIndex(e => new { e.OrganizationId, e.UserId, e.RoleId }).IsUnique();
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => new { e.OrganizationId, e.UserId });

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssignedBy)
            .WithMany()
            .HasForeignKey(e => e.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
