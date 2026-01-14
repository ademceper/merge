using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetLiveStreamQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLiveStreamQueryHandler> logger) : IRequestHandler<GetLiveStreamQuery, LiveStreamDto?>
{
    public async Task<LiveStreamDto?> Handle(GetLiveStreamQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting live stream. StreamId: {StreamId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: AsSplitQuery ile Cartesian Explosion önlenir (birden fazla Include var)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var stream = await context.Set<LiveStream>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ EF Core 9: Query splitting - her Include ayrı sorgu
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return stream != null ? mapper.Map<LiveStreamDto>(stream) : null;
    }
}

