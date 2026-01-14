using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CancelStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelStreamCommandHandler> logger) : IRequestHandler<CancelStreamCommand, Unit>
{
    public async Task<Unit> Handle(CancelStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling stream. StreamId: {StreamId}", request.StreamId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        stream.Cancel();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stream cancelled successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
