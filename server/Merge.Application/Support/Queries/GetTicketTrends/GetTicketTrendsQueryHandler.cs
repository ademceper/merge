using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetTicketTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTicketTrendsQueryHandler : IRequestHandler<GetTicketTrendsQuery, List<TicketTrendDto>>
{
    private readonly IDbContext _context;

    public GetTicketTrendsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketTrendDto>> Handle(GetTicketTrendsQuery request, CancellationToken cancellationToken)
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
        // Note: EF Core doesn't support grouping by Date directly, so we need to use a workaround
        var trends = await query
            .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month, Day = t.CreatedAt.Day })
            .Select(g => new TicketTrendDto(
                new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                g.Count(),
                g.Count(t => t.ResolvedAt.HasValue && 
                           t.ResolvedAt.Value.Year == g.Key.Year &&
                           t.ResolvedAt.Value.Month == g.Key.Month &&
                           t.ResolvedAt.Value.Day == g.Key.Day),
                g.Count(t => t.ClosedAt.HasValue &&
                             t.ClosedAt.Value.Year == g.Key.Year &&
                             t.ClosedAt.Value.Month == g.Key.Month &&
                             t.ClosedAt.Value.Day == g.Key.Day)
            ))
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return trends;
    }
}
