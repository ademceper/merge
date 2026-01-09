using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetUserAuditHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserAuditHistoryQueryHandler : IRequestHandler<GetUserAuditHistoryQuery, IEnumerable<AuditLogDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserAuditHistoryQueryHandler> _logger;

    public GetUserAuditHistoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserAuditHistoryQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetUserAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user audit history. UserId: {UserId}, Days: {Days}",
            request.UserId, request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId &&
                   a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .Take(1000) // ✅ Güvenlik: Maksimum 1000 audit log
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            result.Add(_mapper.Map<AuditLogDto>(audit));
        }
        return result;
    }
}

