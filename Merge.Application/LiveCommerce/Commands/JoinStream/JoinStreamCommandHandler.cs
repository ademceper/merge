using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.JoinStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class JoinStreamCommandHandler : IRequestHandler<JoinStreamCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JoinStreamCommandHandler> _logger;

    public JoinStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<JoinStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(JoinStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Joining stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            _logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // Check if viewer already exists
        var existingViewer = await _context.Set<LiveStreamViewer>()
            .FirstOrDefaultAsync(v => v.LiveStreamId == request.StreamId &&
                (request.UserId.HasValue ? v.UserId == request.UserId : v.GuestId == request.GuestId) &&
                v.IsActive, cancellationToken);

        if (existingViewer != null)
        {
            _logger.LogInformation("Viewer already in stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
                request.StreamId, request.UserId, request.GuestId);
            return Unit.Value; // Already joined
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var viewer = LiveStreamViewer.Create(
            request.StreamId,
            request.UserId,
            request.GuestId);

        await _context.Set<LiveStreamViewer>().AddAsync(viewer, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        stream.IncrementViewerCount();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Joined stream successfully. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);
        return Unit.Value;
    }
}

