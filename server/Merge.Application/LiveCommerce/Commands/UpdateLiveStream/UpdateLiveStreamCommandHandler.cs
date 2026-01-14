using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.UpdateLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class UpdateLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateLiveStreamCommandHandler> logger) : IRequestHandler<UpdateLiveStreamCommand, LiveStreamDto>
{
    public async Task<LiveStreamDto> Handle(UpdateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating live stream. StreamId: {StreamId}", request.StreamId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        stream.UpdateDetails(
            request.Title,
            request.Description,
            request.ScheduledStartTime,
            request.StreamUrl,
            request.StreamKey,
            request.ThumbnailUrl,
            request.Category,
            request.Tags);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        // ✅ PERFORMANCE: AsSplitQuery ile Cartesian Explosion önlenir (birden fazla Include var)
        var updatedStream = await context.Set<LiveStream>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ EF Core 9: Query splitting - her Include ayrı sorgu
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (updatedStream == null)
        {
            logger.LogWarning("Stream not found after update. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        logger.LogInformation("Live stream updated successfully. StreamId: {StreamId}", request.StreamId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<LiveStreamDto>(updatedStream);
    }
}

