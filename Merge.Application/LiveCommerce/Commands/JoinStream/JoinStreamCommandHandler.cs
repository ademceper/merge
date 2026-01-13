using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.JoinStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class JoinStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<JoinStreamCommandHandler> logger) : IRequestHandler<JoinStreamCommand, Unit>
{
    public async Task<Unit> Handle(JoinStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Joining stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // Check if viewer already exists
        var existingViewer = await context.Set<LiveStreamViewer>()
            .FirstOrDefaultAsync(v => v.LiveStreamId == request.StreamId &&
                (request.UserId.HasValue ? v.UserId == request.UserId : v.GuestId == request.GuestId) &&
                v.IsActive, cancellationToken);

        if (existingViewer != null)
        {
            logger.LogInformation("Viewer already in stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
                request.StreamId, request.UserId, request.GuestId);
            return Unit.Value; // Already joined
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var viewer = LiveStreamViewer.Create(
            request.StreamId,
            request.UserId,
            request.GuestId);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Aggregate root üzerinden viewer ekleme (encapsulation)
        // AddViewer method'u içinde IncrementViewerCount() da çağrılıyor
        stream.AddViewer(viewer);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Joined stream successfully. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);
        return Unit.Value;
    }
}

