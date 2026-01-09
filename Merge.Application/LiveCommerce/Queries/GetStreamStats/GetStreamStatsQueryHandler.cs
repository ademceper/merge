using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStreamStatsQueryHandler : IRequestHandler<GetStreamStatsQuery, LiveStreamStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetStreamStatsQueryHandler> _logger;

    public GetStreamStatsQueryHandler(
        IDbContext context,
        ILogger<GetStreamStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LiveStreamStatsDto> Handle(GetStreamStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stream stats. StreamId: {StreamId}", request.StreamId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var stream = await _context.Set<LiveStream>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            _logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ PERFORMANCE: Database'de aggregation (memory'de YASAK)
        var totalViewers = await _context.Set<LiveStreamViewer>()
            .CountAsync(v => v.LiveStreamId == request.StreamId, cancellationToken);

        var totalOrders = await _context.Set<LiveStreamOrder>()
            .CountAsync(o => o.LiveStreamId == request.StreamId, cancellationToken);

        var totalRevenue = await _context.Set<LiveStreamOrder>()
            .Where(o => o.LiveStreamId == request.StreamId)
            .SumAsync(o => (decimal?)o.OrderAmount, cancellationToken) ?? 0;

        var duration = stream.ActualStartTime.HasValue && stream.EndTime.HasValue
            ? (int)(stream.EndTime.Value - stream.ActualStartTime.Value).TotalSeconds
            : stream.ActualStartTime.HasValue
                ? (int)(DateTime.UtcNow - stream.ActualStartTime.Value).TotalSeconds
                : 0;

        return new LiveStreamStatsDto(
            stream.Id,
            stream.ViewerCount,
            stream.PeakViewerCount,
            totalViewers,
            stream.OrderCount,
            stream.Revenue,
            totalRevenue,
            stream.Status.ToString(),
            duration);
    }
}

