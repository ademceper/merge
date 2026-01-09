using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteLiveStreamCommandHandler : IRequestHandler<DeleteLiveStreamCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteLiveStreamCommandHandler> _logger;

    public DeleteLiveStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteLiveStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteLiveStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting live stream. StreamId: {StreamId}", request.StreamId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            _logger.LogWarning("Stream not found for deletion. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        stream.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Live stream deleted successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}

