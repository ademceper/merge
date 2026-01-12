using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Queries.GetCustomerCommunicationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerCommunicationStatsQueryHandler : IRequestHandler<GetCustomerCommunicationStatsQuery, Dictionary<string, int>>
{
    private readonly IDbContext _context;

    public GetCustomerCommunicationStatsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, int>> Handle(GetCustomerCommunicationStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        IQueryable<CustomerCommunication> query = _context.Set<CustomerCommunication>()
            .AsNoTracking();

        if (request.StartDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= request.EndDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var email = await query.CountAsync(c => c.CommunicationType == "Email", cancellationToken);
        var sms = await query.CountAsync(c => c.CommunicationType == "SMS", cancellationToken);
        var ticket = await query.CountAsync(c => c.CommunicationType == "Ticket", cancellationToken);
        var inApp = await query.CountAsync(c => c.CommunicationType == "InApp", cancellationToken);
        var sent = await query.CountAsync(c => c.Status == CommunicationStatus.Sent, cancellationToken);
        var delivered = await query.CountAsync(c => c.Status == CommunicationStatus.Delivered, cancellationToken);
        var read = await query.CountAsync(c => c.Status == CommunicationStatus.Read, cancellationToken);
        var failed = await query.CountAsync(c => c.Status == CommunicationStatus.Failed, cancellationToken);

        return new Dictionary<string, int>
        {
            { "Total", total },
            { "Email", email },
            { "SMS", sms },
            { "Ticket", ticket },
            { "InApp", inApp },
            { "Sent", sent },
            { "Delivered", delivered },
            { "Read", read },
            { "Failed", failed }
        };
    }
}
