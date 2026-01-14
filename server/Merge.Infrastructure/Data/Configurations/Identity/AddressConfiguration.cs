using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasOne(e => e.User)
              .WithMany(e => e.Addresses)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
