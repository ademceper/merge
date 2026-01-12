using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetTicketsByPriority;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTicketsByPriorityQueryHandler : IRequestHandler<GetTicketsByPriorityQuery, List<PriorityTicketCountDto>>
{
    private readonly IDbContext _context;

    public GetTicketsByPriorityQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<PriorityTicketCountDto>> Handle(GetTicketsByPriorityQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

        if (request.AgentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == request.AgentId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        // ✅ PERFORMANCE: Database'de grouping yap, memory'de işlem YASAK
        var total = await query.CountAsync(cancellationToken);

        var grouped = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityTicketCountDto(
                g.Key,
                g.Count(),
                total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            ))
            .OrderByDescending(p => p.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }
}
