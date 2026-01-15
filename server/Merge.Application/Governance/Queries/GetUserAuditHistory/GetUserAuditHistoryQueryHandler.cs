using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetUserAuditHistory;

public class GetUserAuditHistoryQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetUserAuditHistoryQueryHandler> logger) : IRequestHandler<GetUserAuditHistoryQuery, IEnumerable<AuditLogDto>>
{

    public async Task<IEnumerable<AuditLogDto>> Handle(GetUserAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user audit history. UserId: {UserId}, Days: {Days}",
            request.UserId, request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        var audits = await context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId &&
                   a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .Take(1000)
            .ToListAsync(cancellationToken);

        var result = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            result.Add(mapper.Map<AuditLogDto>(audit));
        }
        return result;
    }
}
