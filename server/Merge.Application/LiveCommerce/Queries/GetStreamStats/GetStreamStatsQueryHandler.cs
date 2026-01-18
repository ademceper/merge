using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

public class GetStreamStatsQueryHandler(
    IDbContext context,
    ILogger<GetStreamStatsQueryHandler> logger) : IRequestHandler<GetStreamStatsQuery, LiveStreamStatsDto>
{
    private const int DefaultDurationSeconds = 0;
    private const decimal DefaultRevenue = 0m;

    public async Task<LiveStreamStatsDto> Handle(GetStreamStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting stream stats. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream is null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        var totalViewers = await context.Set<LiveStreamViewer>()
            .CountAsync(v => v.LiveStreamId == request.StreamId, cancellationToken);

        var totalOrders = await context.Set<LiveStreamOrder>()
            .CountAsync(o => o.LiveStreamId == request.StreamId, cancellationToken);

        var totalRevenue = await context.Set<LiveStreamOrder>()
            .Where(o => o.LiveStreamId == request.StreamId)
            .SumAsync(o => (decimal?)o.OrderAmount, cancellationToken) ?? DefaultRevenue;

        var duration = stream.ActualStartTime.HasValue && stream.EndTime.HasValue
            ? (int)(stream.EndTime.Value - stream.ActualStartTime.Value).TotalSeconds
            : stream.ActualStartTime.HasValue
                ? (int)(DateTime.UtcNow - stream.ActualStartTime.Value).TotalSeconds
                : DefaultDurationSeconds;

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
