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

namespace Merge.Application.LiveCommerce.Commands.LeaveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class LeaveStreamCommandHandler : IRequestHandler<LeaveStreamCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveStreamCommandHandler> _logger;

    public LeaveStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<LeaveStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(LeaveStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Leaving stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var viewer = await _context.Set<LiveStreamViewer>()
            .FirstOrDefaultAsync(v => v.LiveStreamId == request.StreamId &&
                (request.UserId.HasValue ? v.UserId == request.UserId : v.GuestId == request.GuestId) &&
                v.IsActive, cancellationToken);

        if (viewer == null)
        {
            _logger.LogWarning("Viewer not found in stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
                request.StreamId, request.UserId, request.GuestId);
            throw new NotFoundException("Yayın izleyicisi", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        viewer.Leave();

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            stream.DecrementViewerCount();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Left stream successfully. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);
        return Unit.Value;
    }
}

