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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAcceptanceCountQueryHandler : IRequestHandler<GetAcceptanceCountQuery, int>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAcceptanceCountQueryHandler> _logger;

    public GetAcceptanceCountQueryHandler(
        IDbContext context,
        ILogger<GetAcceptanceCountQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(GetAcceptanceCountQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving acceptance count. PolicyId: {PolicyId}", request.PolicyId);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        // Not: int için cache kullanılamıyor (ICacheService sadece class türleri için çalışıyor)
        var count = await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .CountAsync(pa => pa.PolicyId == request.PolicyId && pa.IsActive, cancellationToken);

        return count;
    }
}

