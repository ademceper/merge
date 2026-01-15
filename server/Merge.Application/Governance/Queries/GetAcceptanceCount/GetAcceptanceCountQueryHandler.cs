using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Queries.GetAcceptanceCount;

public class GetAcceptanceCountQueryHandler(
    IDbContext context,
    ILogger<GetAcceptanceCountQueryHandler> logger) : IRequestHandler<GetAcceptanceCountQuery, int>
{
    public async Task<int> Handle(GetAcceptanceCountQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving acceptance count. PolicyId: {PolicyId}", request.PolicyId);

        var count = await context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .CountAsync(pa => pa.PolicyId == request.PolicyId && pa.IsActive, cancellationToken);

        return count;
    }
}

