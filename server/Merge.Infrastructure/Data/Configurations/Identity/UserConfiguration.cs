using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.PhoneNumber);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
    }
}
