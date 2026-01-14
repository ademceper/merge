using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Support;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class SupportDbContext : DbContext, IDbContext
{
    public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options)
    {
    }

    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<CustomerCommunication> CustomerCommunications { get; set; }
    public DbSet<LiveChatSession> LiveChatSessions { get; set; }
    public DbSet<LiveChatMessage> LiveChatMessages { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Support");
    }
}
