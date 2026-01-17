using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetCustomerCommunicationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerCommunicationStatsQueryHandler(IDbContext context) : IRequestHandler<GetCustomerCommunicationStatsQuery, Dictionary<string, int>>
{

    public async Task<Dictionary<string, int>> Handle(GetCustomerCommunicationStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        IQueryable<CustomerCommunication> query = context.Set<CustomerCommunication>()
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
