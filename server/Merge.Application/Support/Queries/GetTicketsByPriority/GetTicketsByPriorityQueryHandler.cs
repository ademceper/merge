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

public class GetTicketsByPriorityQueryHandler(IDbContext context) : IRequestHandler<GetTicketsByPriorityQuery, List<PriorityTicketCountDto>>
{

    public async Task<List<PriorityTicketCountDto>> Handle(GetTicketsByPriorityQuery request, CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
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
